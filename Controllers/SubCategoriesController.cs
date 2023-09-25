using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    public class SubCategoriesController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly ISubCategoriesService _SubCategoriesService;
        #endregion

        #region Const
        public SubCategoriesController(
            ILoggerRepository logger,
            ISubCategoriesService SubCategoriesService)
        {
            _logger = logger;
            _SubCategoriesService = SubCategoriesService;
        }
        #endregion


        #region Get Info SubCategories   
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name)
        {
            try
            {
                ResObj res = await _SubCategoriesService.GetAll(Name);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SubCategoriesController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region insert or update Info SubCategories 
        [HttpPost]
        public async Task<IActionResult> Post(SubCategories SubCategories)
        {
            try
            {
                ResObj res;
                res = await _SubCategoriesService.Post(SubCategories);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SubCategoriesController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info SubCategories 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _SubCategoriesService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SubCategoriesController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get SubCategories ById Info SubCategories 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _SubCategoriesService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SubCategoriesController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion 
    }
}
