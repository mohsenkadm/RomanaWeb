using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RomanaWeb.Classes;
using RomanaWeb.Helper;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class DriverDispatchService : IDriverDispatchService, IRegisterScopped
    {
        private static readonly TimeSpan LocationBroadcastThrottle = TimeSpan.FromSeconds(4);
        private static readonly double[] TierRadiiKm = { 1, 2, 3, 4 };

        private readonly DB_Context _context;
        private readonly INotificationDispatcher _notifier;
        private readonly IOrderHubNotifier _hubNotifier;
        private readonly ILoggerRepository _logger;
        private readonly IMemoryCache _cache;
        private readonly IPricingService _pricing;
        private readonly IDistanceService _distance;

        public DriverDispatchService(
            DB_Context context,
            INotificationDispatcher notifier,
            IOrderHubNotifier hubNotifier,
            ILoggerRepository logger,
            IMemoryCache cache,
            IPricingService pricing,
            IDistanceService distance)
        {
            _context = context;
            _notifier = notifier;
            _hubNotifier = hubNotifier;
            _logger = logger;
            _cache = cache;
            _pricing = pricing;
            _distance = distance;
        }

        public async Task<ResObj> DispatchOrder(int orderId, double radiusKm = 5d)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return Result.Return(false, "الطلب غير موجود");
            if (order.IsCancel) return Result.Return(false, "الطلب ملغي");

            var pickup = await _context.Restaurant.AsNoTracking()
                .FirstOrDefaultAsync(r => r.RestaurantId == order.RestaurantId);
            if (pickup == null) return Result.Return(false, "المطعم غير موجود");
            if (!double.TryParse(pickup.Lat, out double pLat) ||
                !double.TryParse(pickup.Long, out double pLng))
                return Result.Return(false, "موقع المطعم غير محدد");

            var settings = await _context.AppSettings.AsNoTracking().FirstOrDefaultAsync()
                           ?? new AppSettings();
            bool allowBusy = settings.AllowBusyDriverDispatch;

            var busyDriverIds = allowBusy
                ? new List<int>()
                : await GetBusyDriverIdsAsync();

            var drivers = await _context.SaleMan.AsNoTracking()
                .Where(d => d.IsActive != false && d.IsDelete != true && d.IsAvailable)
                .Where(d => !busyDriverIds.Contains(d.SaleManId))
                .ToListAsync();

            if (drivers.Count == 0)
                return Result.Return(false, "لا يوجد سائقين متاحين — يرجى التخصيص من لوحة التحكم");

            int? dropoffZoneId = await ResolveOrderDropoffZoneIdAsync(order);

            var driverZones = await _context.SaleManZone.AsNoTracking().ToListAsync();
            var zonesByDriver = driverZones.GroupBy(z => z.SaleManId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ZoneId).ToHashSet());

            var eligible = drivers.Where(d =>
            {
                if (!zonesByDriver.TryGetValue(d.SaleManId, out var zoneSet) || zoneSet.Count == 0)
                    return false;
                return dropoffZoneId.HasValue && zoneSet.Contains(dropoffZoneId.Value);
            }).ToList();

            if (eligible.Count == 0)
                return Result.Return(false, "لا يوجد سائقين لهذا الزون — يرجى التخصيص من لوحة الحالات");

            var ranked = eligible
                .Select(d =>
                {
                    if (string.IsNullOrWhiteSpace(d.Lat) || string.IsNullOrWhiteSpace(d.Long) ||
                        !double.TryParse(d.Lat, out double dLat) ||
                        !double.TryParse(d.Long, out double dLng))
                        return null;
                    return new { Driver = d, Distance = Haversine(pLat, pLng, dLat, dLng) };
                })
                .Where(x => x != null)
                .OrderBy(x => x!.Distance)
                .Select(x => new { x!.Driver.SaleManId, x.Distance })
                .ToList();

            List<int> nearby = new();
            foreach (double tier in TierRadiiKm)
            {
                nearby = ranked.Where(x => x.Distance <= tier).Select(x => x.SaleManId).ToList();
                if (nearby.Count > 0) break;
            }

            if (nearby.Count == 0)
                nearby = ranked.Where(x => x.Distance <= radiusKm).Select(x => x.SaleManId).ToList();

            if (nearby.Count == 0)
                nearby = eligible.Select(d => d.SaleManId).ToList();

            string title = "طلب جديد متاح";
            string body = $"طلب رقم {order.OrderNo} متاح للقبول";
            var payload = new NotificationPayload
            {
                Title = title,
                Body = body,
                Data = new Dictionary<string, string>
                {
                    ["type"] = "new_dispatch",
                    ["orderId"] = orderId.ToString(),
                    ["statusKey"] = "new_order"
                }
            };

            await _notifier.SendManyAsync(NotificationAudience.Driver, nearby, payload);

            try
            {
                foreach (var driverId in nearby)
                    await _hubNotifier.NotifyDriverAsync(driverId, title, body, orderId, "new_order", 1, "banner");
                await _hubNotifier.NotifyAllDriversAsync(title, body, orderId, "new_order", 1, "banner");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverDispatchService => DispatchOrder => SignalR");
            }

            return Result.Return(true, $"تم ارسال الطلب الى {nearby.Count} سائق");
        }

        private async Task<int?> ResolveOrderDropoffZoneIdAsync(Orders order)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == order.UserId);
            string? latStr = !string.IsNullOrWhiteSpace(order.Lat) ? order.Lat : user?.Lat;
            string? lngStr = !string.IsNullOrWhiteSpace(order.Long) ? order.Long : user?.Long;
            if (!_distance.TryParseCoord(latStr, lngStr, out double lat, out double lng))
                return null;

            var (_, zone) = await _pricing.ResolveZoneAtPointAsync(lat, lng);
            return zone?.ZoneId;
        }

        // Section 6: a driver cancels an accepted order ⇒ append reason, free the order, re-dispatch.
        public async Task<ResObj> CancelByDriver(int orderId, int saleManId, string reason)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return Result.Return(false, "الطلب غير موجود");
            if (order.SaleManId != saleManId)
                return Result.Return(false, "هذا الطلب ليس مسندا اليك");

            string stamp = $"[Driver {saleManId} cancel @ {Key.DateTimeIQ:yyyy-MM-dd HH:mm}] {reason}";
            order.Notes = string.IsNullOrWhiteSpace(order.Notes) ? stamp : (order.Notes + Environment.NewLine + stamp);
            order.SaleManId = 0;
            order.IsSaleManApprove = false;
            order.IsSaleManCancel = true;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await ClearActiveOrderAsync(saleManId);

            if (order.UserId > 0)
            {
                await _notifier.SendAsync(NotificationAudience.User, order.UserId, new NotificationPayload
                {
                    Title = "يوجد تأخير بسيط",
                    Body = "نعتذر، هناك ضغط مرتفع على الطلبات حاليا."
                });
            }

            return await DispatchOrder(orderId);
        }

        public async Task<ResObj> UpdateDriverLocation(int saleManId, double lat, double lng, int? orderId = null)
        {
            var driver = await _context.SaleMan.FirstOrDefaultAsync(d => d.SaleManId == saleManId);
            if (driver == null) return Result.Return(false, "السائق غير موجود");

            var now = DateTime.UtcNow;
            var loc = await _context.DriverLocations.FirstOrDefaultAsync(d => d.SaleManId == saleManId);
            if (loc == null)
            {
                loc = new DriverLocation { SaleManId = saleManId };
                await _context.DriverLocations.AddAsync(loc);
            }

            loc.Lat = lat;
            loc.Lng = lng;
            loc.UpdatedAt = now;
            if (orderId is > 0)
                loc.ActiveOrderId = orderId;

            driver.Lat = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            driver.Long = lng.ToString(System.Globalization.CultureInfo.InvariantCulture);
            driver.LocationUpdatedAt = now;
            _context.Entry(driver).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            if (loc.ActiveOrderId is > 0)
                await TryBroadcastDriverLocationAsync(loc.ActiveOrderId.Value, saleManId, lat, lng, now);

            return Result.Return(true, "تم تحديث الموقع");
        }

        public async Task<(bool ok, int httpStatus, object? data, string? msg)> GetDriverLocationForOrder(int orderId, int userId)
        {
            var order = await _context.Orders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
                return (false, StatusCodes.Status404NotFound, null, "الطلب غير موجود");

            if (order.UserId != userId)
                return (false, StatusCodes.Status403Forbidden, null, "غير مصرح لك بعرض موقع هذا الطلب");

            if (order.SaleManId is null or 0)
                return (false, StatusCodes.Status404NotFound, null, "لا يوجد سائق معين لهذا الطلب");

            var loc = await _context.DriverLocations.AsNoTracking()
                .FirstOrDefaultAsync(d => d.SaleManId == order.SaleManId);
            if (loc == null)
                return (false, StatusCodes.Status404NotFound, null, "لا يوجد موقع متاح للسائق");

            return (true, StatusCodes.Status200OK, new
            {
                lat = loc.Lat,
                lng = loc.Lng,
                heading = (double?)null,
                updatedAt = loc.UpdatedAt,
                saleManId = order.SaleManId.Value
            }, null);
        }

        public async Task SetActiveOrderAsync(int saleManId, int orderId)
        {
            var loc = await _context.DriverLocations.FirstOrDefaultAsync(d => d.SaleManId == saleManId);
            if (loc == null)
            {
                loc = new DriverLocation
                {
                    SaleManId = saleManId,
                    Lat = 0,
                    Lng = 0,
                    UpdatedAt = DateTime.UtcNow,
                    ActiveOrderId = orderId
                };
                await _context.DriverLocations.AddAsync(loc);
            }
            else
            {
                loc.ActiveOrderId = orderId;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ClearActiveOrderAsync(int saleManId)
        {
            var loc = await _context.DriverLocations.FirstOrDefaultAsync(d => d.SaleManId == saleManId);
            if (loc == null || loc.ActiveOrderId == null)
                return;

            loc.ActiveOrderId = null;
            _context.Entry(loc).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DriverServesOrderZoneAsync(int saleManId, Orders order)
        {
            var dropoffZoneId = await ResolveOrderDropoffZoneIdAsync(order);
            if (!dropoffZoneId.HasValue) return false;

            var driverZones = await ZoneCoverageHelper.GetDriverZoneIdsAsync(_context, saleManId);
            return ZoneCoverageHelper.ServesZone(driverZones, dropoffZoneId);
        }

        public async Task<bool> DriverHasActiveOrderAsync(int saleManId, int? excludeOrderId = null)
        {
            var query = _context.Orders.AsNoTracking()
                .Where(o => o.SaleManId == saleManId
                    && o.IsSaleManApprove == true
                    && !o.IsDone
                    && !o.IsCancel);

            if (excludeOrderId is > 0)
                query = query.Where(o => o.OrderId != excludeOrderId);

            return await query.AnyAsync();
        }

        private async Task<List<int>> GetBusyDriverIdsAsync()
        {
            return await _context.Orders.AsNoTracking()
                .Where(o => o.SaleManId != null && o.SaleManId > 0
                    && o.IsSaleManApprove == true
                    && !o.IsDone
                    && !o.IsCancel)
                .Select(o => o.SaleManId!.Value)
                .Distinct()
                .ToListAsync();
        }

        private async Task TryBroadcastDriverLocationAsync(int orderId, int saleManId, double lat, double lng, DateTime at)
        {
            var throttleKey = $"loc_throttle_{orderId}";
            if (_cache.TryGetValue(throttleKey, out _))
                return;

            var order = await _context.Orders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null || order.UserId <= 0)
                return;

            try
            {
                await _hubNotifier.NotifyUserDriverLocationUpdatedAsync(order.UserId, orderId, saleManId, lat, lng, at);
                _cache.Set(throttleKey, true, LocationBroadcastThrottle);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverDispatchService => TryBroadcastDriverLocationAsync");
            }
        }

        private static double Haversine(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371d;
            double dLat = (lat2 - lat1) * Math.PI / 180d;
            double dLng = (lng2 - lng1) * Math.PI / 180d;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180d) * Math.Cos(lat2 * Math.PI / 180d) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
