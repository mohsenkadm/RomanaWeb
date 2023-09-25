using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class RestaurantSubCategoriesController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IRestaurantSubCategoriesService _RestaurantSubCategoriesService;
        #endregion

        #region Const
        public RestaurantSubCategoriesController(
            ILoggerRepository logger,
            IRestaurantSubCategoriesService RestaurantSubCategoriesService)
        {
            _logger = logger;
            _RestaurantSubCategoriesService = RestaurantSubCategoriesService;
        }
        #endregion


        #region Get Info RestaurantSubCategories  
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name)
        {
            try
            {
                ResObj res = await _RestaurantSubCategoriesService.GetAll(Name);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantSubCategoriesController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region insert or update Info RestaurantSubCategories 
        [HttpPost]
        public async Task<IActionResult> Post(RestaurantSubCategories RestaurantSubCategories)
        {
            try
            {

                ResObj res;
                res = await _RestaurantSubCategoriesService.Post(RestaurantSubCategories);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantSubCategoriesController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info RestaurantSubCategories 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _RestaurantSubCategoriesService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantSubCategoriesController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get RestaurantSubCategories ById Info RestaurantSubCategories 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _RestaurantSubCategoriesService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantSubCategoriesController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
    }
}
