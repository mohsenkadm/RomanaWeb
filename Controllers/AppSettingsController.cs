using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class AppSettingsController : MasterController
    {
        private readonly ILoggerRepository _logger;
        private readonly IAppSettingsService _appSettingsService;

        public AppSettingsController(ILoggerRepository logger, IAppSettingsService appSettingsService)
        {
            _logger = logger;
            _appSettingsService = appSettingsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                ResObj res = await _appSettingsService.Get();
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AppSettingsController => Get");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(AppSettings settings)
        {
            try
            {
                ResObj res = await _appSettingsService.Update(settings);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AppSettingsController => Update");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePricePerKm(decimal pricePerKm)
        {
            try
            {
                ResObj res = await _appSettingsService.UpdatePricePerKm(pricePerKm);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AppSettingsController => UpdatePricePerKm");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDefaultOrderCost(decimal defaultOrderCost)
        {
            try
            {
                ResObj res = await _appSettingsService.UpdateDefaultOrderCost(defaultOrderCost);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AppSettingsController => UpdateDefaultOrderCost");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }

        /// <summary>
        /// Calculate delivery cost based on customer location (lat/lng) relative to store
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetDeliveryCostByLocation(double storeLat, double storeLng, double customerLat, double customerLng)
        {
            try
            {
                ResObj res = await _appSettingsService.CalculateDeliveryCostByLocation(storeLat, storeLng, customerLat, customerLng);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AppSettingsController => GetDeliveryCostByLocation");
                return Response(false, "حدث خطأ اثناء عملية الحساب");
            }
        }

        /// <summary>
        /// Calculate delivery cost based on distance in km (for store use)
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetDeliveryCostByKm(double distanceKm)
        {
            try
            {
                ResObj res = await _appSettingsService.CalculateDeliveryCostByKm(distanceKm);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AppSettingsController => GetDeliveryCostByKm");
                return Response(false, "حدث خطأ اثناء عملية الحساب");
            }
        }
    }
}
