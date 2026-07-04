using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Controllers
{
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

        [AllowAnonymous]
        [HttpGet("zones/resolve")]
        public async Task<IActionResult> ResolveZone([FromQuery] double lat, [FromQuery] double lng)
        {
            try
            {
                var (covered, zone) = await _pricingService.ResolveZoneAtPointAsync(lat, lng);
                if (!covered || zone == null)
                    return Response(true, new { inCoverage = false, zoneId = (int?)null, zoneName = (string?)null });
                return Response(true, new
                {
                    inCoverage = true,
                    zoneId = zone.ZoneId,
                    zoneName = zone.Name,
                    lzaKm = zone.LzaKm,
                    ecaPricePerKm = zone.EcaPricePerKm
                });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PricingController => ResolveZone");
                return Response(false, "خطأ");
            }
        }

        [AllowAnonymous]
        [HttpGet("coverage/check")]
        public async Task<IActionResult> CoverageCheck([FromQuery] double lat, [FromQuery] double lng)
        {
            try
            {
                var (covered, zone) = await _pricingService.ResolveZoneAtPointAsync(lat, lng);
                return Response(true, new
                {
                    covered,
                    zoneId = zone?.ZoneId,
                    zoneName = zone?.Name,
                    message = covered ? "ضمن مناطق التغطية" : "أنت خارج الزونات المدعومة"
                });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PricingController => CoverageCheck");
                return Response(false, "خطأ");
            }
        }
    }
}
