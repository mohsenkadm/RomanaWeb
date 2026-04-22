using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class DriverStarsController : MasterController
    {
        private readonly IDriverStarsService _service;
        private readonly ILoggerRepository _logger;

        public DriverStarsController(IDriverStarsService service, ILoggerRepository logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? SaleManName)
        {
            try
            {
                var result = await _service.GetAll(SaleManName);
                return Response(true, result.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverStarsController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب المعلومات");
            }
        }

        [AllowAnonymous]
        [HttpGet("DriverStars/GetBySaleManId/{SaleManId}")]
        public async Task<IActionResult> GetBySaleManId(int SaleManId)
        {
            try
            {
                var result = await _service.GetBySaleManId(SaleManId);
                return Response(true, result.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverStarsController => GetBySaleManId");
                return Response(false, "حدث خطأ اثناء عملية جلب المعلومات");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DriverStars driverStars)
        {
            try
            {
                await _service.Post(driverStars);
                return Response(true, "تم الحفظ بنجاح");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverStarsController => Post");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.Delete(id);
                return Response(true, "تم الحذف بنجاح");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverStarsController => Delete");
                return Response(false, "حدث خطأ اثناء عملية الحذف");
            }
        }
    }
}
