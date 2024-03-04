using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class RestaurantSaleManController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IRestaurantSaleManService _RestaurantSaleManService;
        #endregion

        #region Const
        public RestaurantSaleManController(
            ILoggerRepository logger,
            IRestaurantSaleManService RestaurantSaleManService)
        {
            _logger = logger;
            _RestaurantSaleManService = RestaurantSaleManService;
        }
        #endregion


        #region Get Info RestaurantSaleMan  
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name)
        {
            try
            {
                ResObj res = await _RestaurantSaleManService.GetAll(Name);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantSaleManController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region insert or update Info RestaurantSaleMan 
        [HttpPost]
        public async Task<IActionResult> Post(RestaurantSaleMan RestaurantSaleMan)
        {
            try
            {

                ResObj res;
                res = await _RestaurantSaleManService.Post(RestaurantSaleMan);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantSaleManController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info RestaurantSaleMan 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _RestaurantSaleManService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantSaleManController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get RestaurantSaleMan ById Info RestaurantSaleMan 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _RestaurantSaleManService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantSaleManController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion      

        #region Get RestaurantSaleMan ById Info GetByResId 
        [HttpGet("RestaurantSaleMan/GetByResId/{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByResId(int Id)
        {
            try
            {
                ResObj res = await _RestaurantSaleManService.GetByResId(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantSaleManController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
    }
}
