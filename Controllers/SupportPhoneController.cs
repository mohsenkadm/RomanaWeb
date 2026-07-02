using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    public class SupportPhoneController : MasterController
    {
        private readonly ILoggerRepository _logger;
        private readonly ISupportPhoneService _supportPhoneService;

        public SupportPhoneController(ILoggerRepository logger, ISupportPhoneService supportPhoneService)
        {
            _logger = logger;
            _supportPhoneService = supportPhoneService;
        }

        [NonAction]
        private bool IsAdmin() =>
            string.Equals(UserManager?.Role, "admin", StringComparison.OrdinalIgnoreCase);

        [AllowAnonymous]
        [HttpGet("SupportPhone/GetForApp")]
        public async Task<IActionResult> GetForApp(int appType)
        {
            try
            {
                ResObj res = await _supportPhoneService.GetForApp(appType);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SupportPhoneController => GetForApp");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }

        [AllowAnonymous]
        [HttpGet("SupportPhone/GetAllForApps")]
        public async Task<IActionResult> GetAllForApps()
        {
            try
            {
                ResObj res = await _supportPhoneService.GetAllForApps();
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SupportPhoneController => GetAllForApps");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                ResObj res = await _supportPhoneService.GetAll();
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SupportPhoneController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _supportPhoneService.GetById(Id);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SupportPhoneController => GetById");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(SupportPhone supportPhone)
        {
            try
            {
                if (!IsAdmin())
                    return Response(false, "هذه العملية مخصصة لمدير التطبيق فقط");

                ResObj res = await _supportPhoneService.Post(supportPhone);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SupportPhoneController => Post");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                if (!IsAdmin())
                    return Response(false, "هذه العملية مخصصة لمدير التطبيق فقط");

                ResObj res = await _supportPhoneService.Delete(Id);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SupportPhoneController => Delete");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
    }
}
