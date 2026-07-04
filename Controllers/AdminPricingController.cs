using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Controllers
{
    [Authorize]
    [Route("admin/pricing")]
    public class AdminPricingController : MasterController
    {
        private readonly IPricingService _pricing;
        private readonly ILoggerRepository _logger;

        public AdminPricingController(IPricingService pricing, ILoggerRepository logger)
        {
            _pricing = pricing;
            _logger = logger;
        }

        [HttpPost("simulate")]
        public async Task<IActionResult> Simulate([FromBody] QuoteRequest request)
        {
            try
            {
                if (UserManager == null || !string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return Response(false, "غير مصرح");
                request.ForceZonePricing = true;
                var res = await _pricing.Quote(request);
                if (!res.success)
                    return Response(false, res.msg ?? "تعذر الحساب");
                return Response(true, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminPricingController => Simulate");
                return Response(false, "خطأ");
            }
        }
    }
}
