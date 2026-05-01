using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Controllers
{
    // Section 6 - Driver dispatch endpoints.
    [Authorize]
    [Route("dispatch")]
    public class DriverDispatchController : MasterController
    {
        private readonly ILoggerRepository _logger;
        private readonly IDriverDispatchService _dispatch;

        public DriverDispatchController(ILoggerRepository logger, IDriverDispatchService dispatch)
        {
            _logger = logger;
            _dispatch = dispatch;
        }

        public class CancelRequest
        {
            public int SaleManId { get; set; }
            public string Reason { get; set; } = "";
        }

        public class LocationRequest
        {
            public int SaleManId { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        // Admin/system can fire dispatch (e.g., new order arrived).
        [HttpPost("orders/{orderId:int}/dispatch")]
        public async Task<IActionResult> Dispatch(int orderId, [FromQuery] double radius_km = 5d)
        {
            try
            {
                var res = await _dispatch.DispatchOrder(orderId, radius_km);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverDispatchController => Dispatch");
                return Response(false, "حدث خطأ اثناء عملية الارسال");
            }
        }

        // Driver cancels an accepted order ⇒ reason persisted, order re-dispatched.
        [HttpPost("orders/{orderId:int}/cancel")]
        public async Task<IActionResult> Cancel(int orderId, [FromBody] CancelRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.Reason))
                    return Response(false, "يرجى تقديم سبب الالغاء");

                var res = await _dispatch.CancelByDriver(orderId, req.SaleManId, req.Reason);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverDispatchController => Cancel");
                return Response(false, "حدث خطأ اثناء عملية الالغاء");
            }
        }

        // Driver app heartbeat: location update (used by proximity dispatch).
        [HttpPost("driver/location")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationRequest req)
        {
            try
            {
                if (req == null || req.SaleManId <= 0) return Response(false, "بيانات غير صحيحة");
                var res = await _dispatch.UpdateDriverLocation(req.SaleManId, req.Lat, req.Lng);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverDispatchController => UpdateLocation");
                return Response(false, "حدث خطأ اثناء تحديث الموقع");
            }
        }
    }
}
