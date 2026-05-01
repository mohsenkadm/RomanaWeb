using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;
using static NuGet.Packaging.PackagingConstants;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class PromoCodeController : MasterController
    {

        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IPromoCodeService _PromoCodeService;
        private readonly INotificationService _noteService;
        private readonly IRestaurantService _RestaurantService;
        #endregion

        #region Const
        public PromoCodeController(
            ILoggerRepository logger,
            IPromoCodeService PromoCodeService,
            INotificationService noteService,IRestaurantService restaurantService)
        {
            _logger = logger;
            _PromoCodeService = PromoCodeService;
            _noteService = noteService;
            _RestaurantService = restaurantService;
        }
        #endregion


        #region Get Info PromoCode  
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(string? Name,int? ResId)
        {
            try
            {
                ResObj res = await _PromoCodeService.GetAll(Name, ResId);

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
                // Section 3: Promo codes are created by Admin only.
                if (UserManager == null || !string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return Response(false, "غير مصرح، هذه العملية للأدمن فقط");

                // Section 3.1: STORE-scoped promo codes require a store; GLOBAL ones don't.
                if (!PromoCode.IsForAllStores && PromoCode.RestaurantId == 0)
                    return Response(false, "يجب اختيار المطعم");

                if (!PromoCode.IsForAllStores)
                {
                    var rest = await _RestaurantService.GetById(PromoCode.RestaurantId);
                }

                ResObj res;
                res = await _PromoCodeService.Post(PromoCode);
                Notification notifications = new Notification
                {
                    Title = "برومو كود",
                    Details = $" برومو ",           
                };                                      
                try
                {
                    await OneSignalSender(notifications.Title, notifications.Details);
                }
                catch (Exception ex) { }
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PromoCodeController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion
        #region insert or update Info PromoCode 
        [HttpPost]
        public async Task<IActionResult> PostApp([FromForm]PromoCode PromoCode)
        {
            try
            {

                if (PromoCode.RestaurantId == 0) return Response(false, "يجب اختيار المطعم");

                var rest = await _RestaurantService.GetById(PromoCode.RestaurantId);

                ResObj res;
                res = await _PromoCodeService.Post(PromoCode);
                Restaurant resname = await _RestaurantService.GetRestaurantById(PromoCode.RestaurantId);

                Notification notifications = new Notification
                {
                    Title = "برومو كود",
                    Details = $"مرحباً عزيزنا متوفر خصم في {resname.Name} سارع في التسوق الان "
                    ,ResId=0,UserId=0,SaleManId=0
                };
                try
                {
                    await _noteService.Post(notifications);
                    await OneSignalSender(notifications.Title, notifications.Details);
                }
                catch (Exception ex) { }
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
                if (UserManager == null || !string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return Response(false, "غير مصرح، هذه العملية للأدمن فقط");

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

        #region Get PromoCode ById Info GetByResId 
        [HttpGet("PromoCode/GetByResId/{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByResId(int Id)
        {
            try
            {
                ResObj res = await _PromoCodeService.GetByResId(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PromoCodeController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Validate PromoCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ValidatePromoCode(string promoName, int restaurantId)
        {
            try
            {
                ResObj res = await _PromoCodeService.ValidatePromoCode(promoName, restaurantId);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PromoCodeController => ValidatePromoCode");
                return Response(false, "حدث خطأ اثناء عملية التحقق");
            }
        }
        #endregion

        #region Apply PromoCode to order
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ApplyPromoCode(string promoName, int restaurantId, decimal orderTotal)
        {
            try
            {
                ResObj res = await _PromoCodeService.ApplyPromoCode(promoName, restaurantId, orderTotal);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PromoCodeController => ApplyPromoCode");
                return Response(false, "حدث خطأ اثناء عملية تطبيق الخصم");
            }
        }
        #endregion

        #region PromoCode Analytics
        [HttpGet]
        public async Task<IActionResult> GetAnalytics(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                ResObj res = await _PromoCodeService.GetAnalytics(dateFrom, dateTo);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PromoCodeController => GetAnalytics");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion
    }
}
