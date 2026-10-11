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
    // Section 4 - Admin Order Management.
    // 4.1: Inside an order admin can add/remove products. On Save we cancel the old
    //      order and create+dispatch a new one, then notify merchant + (if assigned) driver.
    [Authorize]
    [Route("admin/orders")]
    public class AdminOrdersController : MasterController
    {
        private readonly ILoggerRepository _logger;
        private readonly IOrdersService _ordersService;
        private readonly INotificationDispatcher _notifier;
        private readonly IDriverDispatchService _dispatch;
        private readonly IOrderHubNotifier _hubNotifier;
        private readonly DB_Context _context;
        private readonly IOrderLogService _orderLog;

        public AdminOrdersController(
            ILoggerRepository logger,
            IOrdersService ordersService,
            INotificationDispatcher notifier,
            IDriverDispatchService dispatch,
            IOrderHubNotifier hubNotifier,
            DB_Context context,
            IOrderLogService orderLog)
        {
            _logger = logger;
            _ordersService = ordersService;
            _notifier = notifier;
            _dispatch = dispatch;
            _hubNotifier = hubNotifier;
            _context = context;
            _orderLog = orderLog;
        }

        public class ReplaceOrderRequest
        {
            public List<OrderDetail> NewDetails { get; set; } = new();
            public double? DispatchRadiusKm { get; set; }
        }

        public class ReassignDriverRequest
        {
            public int SaleManId { get; set; }
        }

        [HttpPost("{id:int}/replace")]
        public async Task<IActionResult> Replace(int id, [FromBody] ReplaceOrderRequest req)
        {
            try
            {
                if (UserManager == null || !string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return Response(false, "غير مصرح، هذه العملية للأدمن فقط");

                if (req == null || req.NewDetails == null || req.NewDetails.Count == 0)
                    return Response(false, "يجب اضافة منتجات للطلب الجديد");

                // Capture old order's driver before replacement so we can notify them.
                var oldRes = await _ordersService.GetById(id);
                int oldDriverId = 0;
                int restaurantId = 0;
                if (oldRes.success && oldRes.data is Orders oldOrder)
                {
                    oldDriverId = oldOrder.SaleManId ?? 0;
                    restaurantId = oldOrder.RestaurantId;
                }

                int? oldOrderNo = null;
                if (oldRes.success && oldRes.data is Orders oo) oldOrderNo = oo.OrderNo;

                var res = await _ordersService.AdminReplaceOrder(id, req.NewDetails);
                if (!res.success) return Response(false, res.msg);

                var newOrder = res.data as Orders;

                await _orderLog.LogAsync(id, "replaced", "استبدال الطلب",
                    $"تم استبدال الطلب بطلب جديد رقم {newOrder?.OrderNo}", oldOrderNo);
                if (newOrder != null)
                {
                    await _orderLog.LogAsync(newOrder.OrderId, "created_replacement", "أنشئ بديلاً لطلب سابق",
                        $"بديل عن الطلب رقم {oldOrderNo ?? id}", newOrder.OrderNo);
                }

                // Notify merchant (Section 4.1) about the replacement.
                if (restaurantId > 0)
                {
                    await _notifier.SendAsync(NotificationAudience.Restaurant, restaurantId, new NotificationPayload
                    {
                        Title = "تم تعديل الطلب",
                        Body = $"تم تعديل الطلب من قبل الادمن. رقم الطلب الجديد: {newOrder?.OrderNo}"
                    });
                }

                // Notify the previously-assigned driver that their assignment was cancelled.
                if (oldDriverId > 0)
                {
                    await _notifier.SendAsync(NotificationAudience.Driver, oldDriverId, new NotificationPayload
                    {
                        Title = "تم الغاء التعيين",
                        Body = $"تم تعديل الطلب رقم {id} من قبل الادمن وسيتم اعادة الارسال."
                    });
                }

                // Re-dispatch the new order to nearby drivers (Section 6).
                if (newOrder != null)
                {
                    await _dispatch.DispatchOrder(newOrder.OrderId, req.DispatchRadiusKm ?? 5d);
                }

                return Response(true, "تم استبدال الطلب", newOrder);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminOrdersController => Replace");
                return Response(false, "حدث خطأ اثناء استبدال الطلب");
            }
        }

        /// <summary>Full audit timeline for an order.</summary>
        [HttpGet("{id:int}/logs")]
        public async Task<IActionResult> GetLogs(int id)
        {
            try
            {
                if (UserManager == null || !string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return Response(false, "غير مصرح");

                var order = await _context.Orders.AsNoTracking()
                    .Where(o => o.OrderId == id)
                    .Select(o => new { o.OrderId, o.OrderNo, o.OrderDate, o.SaleManId })
                    .FirstOrDefaultAsync();

                var logs = await _context.OrderLogs.AsNoTracking()
                    .Where(l => l.OrderId == id)
                    .OrderBy(l => l.CreatedAt)
                    .ThenBy(l => l.OrderLogId)
                    .Select(l => new
                    {
                        l.OrderLogId,
                        l.OrderId,
                        l.OrderNo,
                        l.Action,
                        l.Title,
                        l.Details,
                        l.ActorType,
                        l.ActorId,
                        l.ActorName,
                        l.CreatedAt
                    })
                    .ToListAsync();

                return Response(true, new
                {
                    order = order == null
                        ? new { orderId = id, orderNo = (int?)null, orderDate = (DateTime?)null }
                        : new { orderId = order.OrderId, orderNo = (int?)order.OrderNo, orderDate = (DateTime?)order.OrderDate },
                    logs
                });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminOrdersController => GetLogs");
                return Response(false, "حدث خطأ اثناء جلب سجل الطلب");
            }
        }

        /// <summary>Drivers list for admin reassign modal (name, phone, idle/busy).</summary>
        [HttpGet("drivers")]
        public async Task<IActionResult> Drivers()
        {
            try
            {
                if (UserManager == null || !string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return Response(false, "غير مصرح");

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
                    bool isIdle = active < max;
                    return new
                    {
                        d.SaleManId,
                        d.Name,
                        d.Phone,
                        d.IsAvailable,
                        activeOrders = active,
                        maxConcurrent = max,
                        isIdle
                    };
                }).ToList();

                return Response(true, result);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminOrdersController => Drivers");
                return Response(false, "خطأ");
            }
        }

        [HttpPost("{id:int}/reassign-driver")]
        public async Task<IActionResult> ReassignDriver(int id, [FromBody] ReassignDriverRequest req)
        {
            try
            {
                if (UserManager == null || !string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return Response(false, "غير مصرح، هذه العملية للأدمن فقط");

                if (req == null || req.SaleManId <= 0)
                    return Response(false, "اختر مندوباً");

                var oldRes = await _ordersService.GetById(id);
                int previousDriverId = 0;
                int userId = 0;
                int orderNo = 0;
                if (oldRes.success && oldRes.data is Orders oldOrder)
                {
                    previousDriverId = oldOrder.SaleManId ?? 0;
                    userId = oldOrder.UserId;
                    orderNo = oldOrder.OrderNo;
                }

                var res = await _ordersService.AdminReassignDriver(id, req.SaleManId);
                if (!res.success) return Response(false, res.msg);

                int statusCode = 3;
                try
                {
                    if (previousDriverId > 0 && previousDriverId != req.SaleManId)
                    {
                        await _hubNotifier.NotifyDriverAsync(previousDriverId,
                            "تم الغاء التعيين",
                            $"تم نقل الطلب رقم {orderNo} إلى مندوب آخر من لوحة التحكم",
                            id, "driver_unassigned", statusCode, "banner");
                        await _notifier.SendAsync(NotificationAudience.Driver, previousDriverId, new NotificationPayload
                        {
                            Title = "تم الغاء التعيين",
                            Body = $"تم نقل الطلب رقم {orderNo} إلى مندوب آخر"
                        });
                    }

                    await _hubNotifier.NotifyDriverAsync(req.SaleManId,
                        "تعيين يدوي",
                        $"تم تعيين الطلب رقم {orderNo} لك من لوحة التحكم",
                        id, "driver_assigned", statusCode, "banner");
                    await _notifier.SendAsync(NotificationAudience.Driver, req.SaleManId, new NotificationPayload
                    {
                        Title = "تعيين يدوي",
                        Body = $"تم تعيين الطلب رقم {orderNo} لك"
                    });

                    if (userId > 0)
                    {
                        await _hubNotifier.NotifyUserAsync(userId,
                            "تم تعيين سائق",
                            "تم تعيين مندوب لطلبك",
                            id, "driver_assigned", statusCode, "banner");
                    }
                }
                catch { }

                return Response(true, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminOrdersController => ReassignDriver");
                return Response(false, "حدث خطأ اثناء تبديل المندوب");
            }
        }
    }
}
