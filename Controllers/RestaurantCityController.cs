using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{                          
    public class RestaurantCityController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IRestaurantCityService _RestaurantCityService;
        #endregion

        #region Const
        public RestaurantCityController(
            ILoggerRepository logger,
            IRestaurantCityService RestaurantCityService)
        {
            _logger = logger;
            _RestaurantCityService = RestaurantCityService;
        }
        #endregion
                                 
        #region Get Info RestaurantCity  
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name)
        {
            try
            {
                ResObj res = await _RestaurantCityService.GetAll(Name);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantCityController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region insert or update Info RestaurantCity 
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(RestaurantCity RestaurantCity)
        {
            try
            {

                ResObj res;
                res = await _RestaurantCityService.Post(RestaurantCity);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantCityController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion  
        #region PostFromApp insert or update Info RestaurantCity 
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostFromApp([FromBody] RestaurantCity RestaurantCity)
        {
            try
            {

                ResObj res;
                res = await _RestaurantCityService.Post(RestaurantCity);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantCityController => PostFromApp => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion             


        #region PostFromApp all insert or update Info RestaurantCity 
        [HttpPost("RestaurantCity/PostFromAppAll/{ResId}/{CostDelivery}")]
        [Authorize]
        public async Task<IActionResult> PostFromAppAll(int ResId,decimal CostDelivery)
        {
            try
            {

                ResObj res;
                res = await _RestaurantCityService.PostFromAppAll(ResId, CostDelivery);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantCityController => PostFromApp => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion
        #region delete Info RestaurantCity 
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _RestaurantCityService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantCityController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get RestaurantCity ById Info RestaurantCity 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _RestaurantCityService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantCityController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion      

        #region Get RestaurantCity ById Info GetByResId 
        [HttpGet("RestaurantCity/GetByResId/{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByResId(int Id)
        {
            try
            {
                ResObj res = await _RestaurantCityService.GetByResId(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantCityController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion      

        #region Get delivery fee for restaurant + city
        [HttpGet("RestaurantCity/GetDeliveryFee/{restaurantId}/{cityId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDeliveryFee(int restaurantId, int cityId)
        {
            try
            {
                ResObj res = await _RestaurantCityService.GetDeliveryFee(restaurantId, cityId);
                if (!res.success)
                    return Response(false, res.msg ?? "لا يوجد سعر توصيل لهذه المنطقة");
                return Response(true, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantCityController => GetDeliveryFee");
                return Response(false, "حدث خطأ");
            }
        }
        #endregion
    }
}
