using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    /// <summary>PDF §2.3.1 — لوحة طوارئ: طلبات موافق عليها بدون مندوب.</summary>
    [Authorize]
    [Route("emergency-orders")]
    public class EmergencyOrdersController : MasterController
    {
        private readonly DB_Context _context;
        private readonly ILoggerRepository _logger;
        private readonly IDriverDispatchService _dispatch;
        private readonly IOrderHubNotifier _hubNotifier;
        private readonly IOrderLogService _orderLog;

        public EmergencyOrdersController(
            DB_Context context,
            ILoggerRepository logger,
            IDriverDispatchService dispatch,
            IOrderHubNotifier hubNotifier,
            IOrderLogService orderLog)
        {
            _context = context;
            _logger = logger;
            _dispatch = dispatch;
            _hubNotifier = hubNotifier;
            _orderLog = orderLog;
        }

        private bool IsAdmin() =>
            UserManager != null && string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");

                var rows = await _context.Orders.AsNoTracking()
                    .Where(o => o.IsApporve && !o.IsCancel && !o.IsDone
                                && (o.SaleManId == null || o.SaleManId == 0))
                    .OrderByDescending(o => o.OrderDate)
                    .Take(100)
                    .ToListAsync();

                var restaurantIds = rows.Select(o => o.RestaurantId).Distinct().ToList();
                var restaurants = await _context.Restaurant.AsNoTracking()
                    .Where(r => restaurantIds.Contains(r.RestaurantId))
                    .ToDictionaryAsync(r => r.RestaurantId, r => r.Name);

                var userIds = rows.Select(o => o.UserId).Distinct().ToList();
                var users = await _context.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.UserId))
                    .ToDictionaryAsync(u => u.UserId, u => u);

                var result = rows.Select(o =>
                {
                    users.TryGetValue(o.UserId, out var user);
                    restaurants.TryGetValue(o.RestaurantId, out var resName);
                    return new
                    {
                        o.OrderId,
                        o.OrderNo,
                        o.OrderDate,
                        o.RestaurantId,
                        restaurantName = resName ?? "-",
                        o.UserId,
                        userName = o.UserName ?? user?.Name ?? "-",
                        phone = o.Phone ?? user?.Phone ?? "-",
                        address = o.Address ?? user?.Address ?? "-",
                        o.NetAmount,
                        o.CostDelivery,
                        o.IsPreparing,
                        waitingMinutes = (int)Math.Max(0, (Key.DateTimeIQ - o.OrderDate).TotalMinutes)
                    };
                }).ToList();

                return Response(true, result);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "EmergencyOrdersController => List");
                return Response(false, "خطأ");
            }
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> Drivers()
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");

                var drivers = await _context.SaleMan.AsNoTracking()
                    .Where(s => s.IsDelete != true && s.IsActive != false)
                    .OrderBy(s => s.Name)
                    .Select(s => new
                    {
                        s.SaleManId,
                        s.Name,
                        s.Phone,
                        s.IsAvailable,
                        s.AllowMultiOrders,
                        s.MaxConcurrentOrders
                    })
                    .ToListAsync();

                var activeCounts = await _context.Orders.AsNoTracking()
                    .Where(o => o.SaleManId != null && o.SaleManId > 0
                        && o.IsSaleManApprove == true
                        && !o.IsDone
                        && !o.IsCancel)
                    .GroupBy(o => o.SaleManId!.Value)
                    .Select(g => new { SaleManId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.SaleManId, x => x.Count);

                var result = drivers.Select(d =>
                {
                    activeCounts.TryGetValue(d.SaleManId, out var active);
                    int max = d.AllowMultiOrders ? Math.Max(2, d.MaxConcurrentOrders) : 1;
                    return new
                    {
                        d.SaleManId,
                        d.Name,
                        d.Phone,
                        d.IsAvailable,
                        activeOrders = active,
                        maxConcurrent = max,
                        isIdle = active < max
                    };
                }).ToList();

                return Response(true, result);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "EmergencyOrdersController => Drivers");
                return Response(false, "خطأ");
            }
        }

        /// <summary>On-shift drivers with zero active orders (idle capacity).</summary>
        [HttpGet("idle-drivers")]
        public async Task<IActionResult> IdleDrivers()
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");

                var busyIds = await _context.Orders.AsNoTracking()
                    .Where(o => o.SaleManId != null && o.SaleManId > 0
                        && o.IsSaleManApprove == true
                        && !o.IsDone
                        && !o.IsCancel)
                    .Select(o => o.SaleManId!.Value)
                    .Distinct()
                    .ToListAsync();

                var idle = await _context.SaleMan.AsNoTracking()
                    .Where(s => s.IsDelete != true
                        && s.IsActive != false
                        && s.IsAvailable
                        && !busyIds.Contains(s.SaleManId))
                    .OrderBy(s => s.Name)
                    .Select(s => new
                    {
                        s.SaleManId,
                        s.Name,
                        s.Phone,
                        s.IsAvailable,
                        activeOrders = 0
                    })
                    .ToListAsync();

                var idleIds = idle.Select(d => d.SaleManId).ToList();
                var locations = await _context.DriverLocations.AsNoTracking()
                    .Where(l => idleIds.Contains(l.SaleManId))
                    .ToDictionaryAsync(l => l.SaleManId, l => l);

                var result = idle.Select(d =>
                {
                    locations.TryGetValue(d.SaleManId, out var loc);
                    return new
                    {
                        d.SaleManId,
                        d.Name,
                        d.Phone,
                        d.IsAvailable,
                        d.activeOrders,
                        lat = loc?.Lat,
                        lng = loc?.Lng,
                        locationUpdatedAt = loc?.UpdatedAt
                    };
                }).ToList();

                return Response(true, result);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "EmergencyOrdersController => IdleDrivers");
                return Response(false, "خطأ");
            }
        }

        public class AssignDriverRequest
        {
            public int SaleManId { get; set; }
        }

        [HttpPost("{orderId:int}/assign")]
        public async Task<IActionResult> AssignDriver(int orderId, [FromBody] AssignDriverRequest req)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                if (req == null || req.SaleManId <= 0)
                    return Response(false, "اختر مندوباً");

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null) return Response(false, "الطلب غير موجود");
                if (order.IsCancel) return Response(false, "الطلب ملغي");
                if (order.SaleManId is > 0)
                    return Response(false, "الطلب مُسند لمندوب بالفعل");

                var driver = await _context.SaleMan.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SaleManId == req.SaleManId && s.IsDelete != true);
                if (driver == null) return Response(false, "المندوب غير موجود");
                if (driver.IsActive == false)
                    return Response(false, "المندوب غير نشط — لا يمكن تعيينه على طلبات");
                if (!driver.IsAvailable)
                    return Response(false, "المندوب متوقف عن العمل حالياً");

                if (!await _dispatch.DriverCanAcceptMoreOrdersAsync(req.SaleManId, orderId))
                {
                    var max = DriverDispatchService.EffectiveMaxConcurrentOrders(driver);
                    return Response(false,
                        max <= 1
                            ? "المندوب لديه طلب نشط"
                            : $"المندوب وصل للحد الأقصى ({max}) من الطلبات المتزامنة");
                }

                if (!await _dispatch.DriverServesOrderZoneAsync(req.SaleManId, order))
                    return Response(false, "المندوب لا يعمل في زون موقع الزبون");

                if (!await _dispatch.DriverServesOrderRestaurantAsync(req.SaleManId, order))
                    return Response(false, "المندوب غير مخصص لمطعم هذا الطلب");

                order.SaleManId = req.SaleManId;
                order.IsSaleManApprove = true;
                order.IsSaleManCancel = false;
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await _dispatch.SetActiveOrderAsync(req.SaleManId, orderId);

                int statusCode = OrdersService.MapOrderStatus(order);
                try
                {
                    await _hubNotifier.NotifyDriverAsync(req.SaleManId,
                        "تعيين يدوي", $"تم تعيين الطلب رقم {order.OrderNo} لك من لوحة الطوارئ",
                        orderId, "driver_assigned", statusCode, "banner");
                    await _hubNotifier.NotifyUserAsync(order.UserId,
                        "تم تعيين سائق", "تم تعيين مندوب لطلبك",
                        orderId, "driver_assigned", statusCode, "banner");
                }
                catch { }

                return Response(true, "تم تعيين المندوب", new { order.OrderId, order.SaleManId, driver.Name });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "EmergencyOrdersController => AssignDriver");
                return Response(false, "خطأ");
            }
        }

        [HttpPost("{orderId:int}/redispatch")]
        public async Task<IActionResult> Redispatch(int orderId, [FromQuery] double radius_km = 5d)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");

                var order = await _context.Orders.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null) return Response(false, "الطلب غير موجود");
                if (order.SaleManId is > 0)
                    return Response(false, "الطلب مُسند لمندوب — لا يمكن إعادة الإرسال");

                await _orderLog.LogAsync(orderId, "redispatch", "إعادة إرسال من لوحة الطوارئ",
                    $"نصف قطر البحث: {radius_km} كم", order.OrderNo);

                var res = await _dispatch.DispatchOrder(orderId, radius_km);
                return Response(res.success, res.msg ?? (res.success ? "تم الإرسال" : "فشل الإرسال"));
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "EmergencyOrdersController => Redispatch");
                return Response(false, "خطأ");
            }
        }
    }
}
