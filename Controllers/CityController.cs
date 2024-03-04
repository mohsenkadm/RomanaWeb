using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    public class CityController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly ICityService _CityService;
        #endregion

        #region Const
        public CityController(
            ILoggerRepository logger,
            ICityService CityService)
        {
            _logger = logger;
            _CityService = CityService;
        }
        #endregion
                 
        #region Get Info City   
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name)
        {
            try
            {
                ResObj res = await _CityService.GetAll(Name);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CityController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion  
        #region Get Info City   
        [HttpGet]
        public async Task<IActionResult> GetByCountriesId(int CountriesId)
        {
            try
            {
                ResObj res = await _CityService.GetByCountriesId(CountriesId);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CityController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion
 
        #region insert or update Info City 
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(City City)
        {
            try
            {
                ResObj res;
                res = await _CityService.Post(City);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CityController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info City 
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _CityService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CityController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get City ById Info City 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _CityService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CityController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion 
    }
}
