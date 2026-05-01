using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Controllers
{
    // Section 8 - "Data Analysis" dashboard endpoints (admin-only).
    [Authorize]
    [Route("analytics")]
    public class AnalyticsController : MasterController
    {
        private readonly ILoggerRepository _logger;
        private readonly IAnalyticsService _analytics;

        public AnalyticsController(ILoggerRepository logger, IAnalyticsService analytics)
        {
            _logger = logger;
            _analytics = analytics;
        }

        private bool IsAdmin() =>
            UserManager != null && string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        [HttpGet("orders")]
        public async Task<IActionResult> Orders(DateTime from, DateTime to, string bucket = "day")
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var res = await _analytics.OrdersOverTime(from, to, bucket);
                return Response(res.success, res.data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "Analytics => Orders"); return Response(false, "خطأ"); }
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> Revenue(DateTime from, DateTime to)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var res = await _analytics.Revenue(from, to);
                return Response(res.success, res.data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "Analytics => Revenue"); return Response(false, "خطأ"); }
        }

        [HttpGet("discounts/funding")]
        public async Task<IActionResult> DiscountFunding(DateTime from, DateTime to)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var res = await _analytics.DiscountFunding(from, to);
                return Response(res.success, res.data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "Analytics => DiscountFunding"); return Response(false, "خطأ"); }
        }

        [HttpGet("top/stores")]
        public async Task<IActionResult> TopStores(DateTime from, DateTime to, int take = 10)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var res = await _analytics.TopStores(from, to, take);
                return Response(res.success, res.data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "Analytics => TopStores"); return Response(false, "خطأ"); }
        }

        [HttpGet("top/drivers")]
        public async Task<IActionResult> TopDrivers(DateTime from, DateTime to, int take = 10)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var res = await _analytics.TopDrivers(from, to, take);
                return Response(res.success, res.data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "Analytics => TopDrivers"); return Response(false, "خطأ"); }
        }

        [HttpGet("top/promos")]
        public async Task<IActionResult> TopPromos(DateTime from, DateTime to, int take = 10)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var res = await _analytics.TopPromoCodes(from, to, take);
                return Response(res.success, res.data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "Analytics => TopPromos"); return Response(false, "خطأ"); }
        }

        [HttpGet("delivery")]
        public async Task<IActionResult> Delivery(DateTime from, DateTime to)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var res = await _analytics.DeliveryStats(from, to);
                return Response(res.success, res.data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "Analytics => Delivery"); return Response(false, "خطأ"); }
        }

        [HttpGet("ratings")]
        public async Task<IActionResult> Ratings(DateTime from, DateTime to)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var res = await _analytics.RatingsOverview(from, to);
                return Response(res.success, res.data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "Analytics => Ratings"); return Response(false, "خطأ"); }
        }
    }
}
