using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Controllers
{
    // Section 2.3: POST /pricing/quote returns the price breakdown used by the
    // Customer app order-summary screen.
    [Route("pricing")]
    public class PricingController : MasterController
    {
        private readonly ILoggerRepository _logger;
        private readonly IPricingService _pricingService;

        public PricingController(ILoggerRepository logger, IPricingService pricingService)
        {
            _logger = logger;
            _pricingService = pricingService;
        }

        [AllowAnonymous]
        [HttpPost("quote")]
        public async Task<IActionResult> Quote([FromBody] QuoteRequest request)
        {
            try
            {
                ResObj res = await _pricingService.Quote(request);
                if (!res.success)
                    return Response(false, res.msg ?? "تعذر حساب السعر");
                return Response(true, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PricingController => Quote");
                return Response(false, "حدث خطأ اثناء عملية حساب السعر");
            }
        }
    }
}
