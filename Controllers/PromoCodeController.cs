using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    public class PromoCodeController : MasterController
    {

        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IPromoCodeService _PromoCodeService;
        #endregion

        #region Const
        public PromoCodeController(
            ILoggerRepository logger,
            IPromoCodeService PromoCodeService)
        {
            _logger = logger;
            _PromoCodeService = PromoCodeService;
        }
        #endregion


        #region Get Info PromoCode  
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                ResObj res = await _PromoCodeService.GetAll();

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PromoCodeController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region insert or update Info PromoCode 
        [HttpPost]
        public async Task<IActionResult> Post(PromoCode PromoCode)
        {
            try
            {

                ResObj res;
                res = await _PromoCodeService.Post(PromoCode);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PromoCodeController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info PromoCode 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _PromoCodeService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PromoCodeController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get PromoCode ById Info PromoCode 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _PromoCodeService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PromoCodeController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion 
    }
}
