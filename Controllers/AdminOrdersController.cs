using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
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

        public AdminOrdersController(
            ILoggerRepository logger,
            IOrdersService ordersService,
            INotificationDispatcher notifier,
            IDriverDispatchService dispatch)
        {
            _logger = logger;
            _ordersService = ordersService;
            _notifier = notifier;
            _dispatch = dispatch;
        }

        public class ReplaceOrderRequest
        {
            public List<OrderDetail> NewDetails { get; set; } = new();
            public double? DispatchRadiusKm { get; set; }
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

                var res = await _ordersService.AdminReplaceOrder(id, req.NewDetails);
                if (!res.success) return Response(false, res.msg);

                var newOrder = res.data as Orders;

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
    }
}
