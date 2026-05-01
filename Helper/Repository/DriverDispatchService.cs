using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class DriverDispatchService : IDriverDispatchService, IRegisterScopped
    {
        private readonly DB_Context _context;
        private readonly INotificationDispatcher _notifier;

        public DriverDispatchService(DB_Context context, INotificationDispatcher notifier)
        {
            _context = context;
            _notifier = notifier;
        }

        // Section 6: push the order to every nearby driver. First to accept wins.
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

            // Section 6: working/stopped flag - drivers with IsAvailable=false are
            // off-shift and must not receive any dispatch notification.
            var drivers = await _context.SaleMan.AsNoTracking()
                .Where(d => d.IsActive == true && d.IsDelete != true && d.IsAvailable
                            && d.Lat != null && d.Long != null)
                .ToListAsync();

            var nearby = drivers
                .Select(d =>
                {
                    if (!double.TryParse(d.Lat, out double dLat) ||
                        !double.TryParse(d.Long, out double dLng))
                        return null;
                    return new { Driver = d, Distance = Haversine(pLat, pLng, dLat, dLng) };
                })
                .Where(x => x != null && x!.Distance <= radiusKm)
                .OrderBy(x => x!.Distance)
                .Select(x => x!.Driver.SaleManId)
                .ToList();

            if (nearby.Count == 0)
                return Result.Return(false, "لا يوجد سائقين قريبين");

            await _notifier.SendManyAsync(NotificationAudience.Driver, nearby, new NotificationPayload
            {
                Title = "طلب جديد متاح",
                Body = $"طلب رقم {order.OrderNo} متاح للقبول",
                Data = new Dictionary<string, string> { ["orderId"] = orderId.ToString() }
            });

            return Result.Return(true, $"تم ارسال الطلب الى {nearby.Count} سائق");
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

            // Notify customer of the delay (Section 7 audience: User).
            if (order.UserId > 0)
            {
                await _notifier.SendAsync(NotificationAudience.User, order.UserId, new NotificationPayload
                {
                    Title = "تأخير في الطلب",
                    Body = "نعتذر عن التأخير، نبحث عن سائق جديد لطلبك."
                });
            }

            // Re-dispatch.
            return await DispatchOrder(orderId);
        }

        public async Task<ResObj> UpdateDriverLocation(int saleManId, double lat, double lng)
        {
            var driver = await _context.SaleMan.FirstOrDefaultAsync(d => d.SaleManId == saleManId);
            if (driver == null) return Result.Return(false, "السائق غير موجود");
            driver.Lat = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            driver.Long = lng.ToString(System.Globalization.CultureInfo.InvariantCulture);
            driver.LocationUpdatedAt = Key.DateTimeIQ;
            _context.Entry(driver).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم تحديث الموقع");
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
