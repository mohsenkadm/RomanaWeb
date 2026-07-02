using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;
using RomanaWeb.UploadService;

namespace RomanaWeb.Controllers
{
    public class AppSplashController : MasterController
    {
        private readonly IAppSplashService _splash;
        private readonly ILoggerRepository _logger;
        private readonly IStorageServices _storage;
        private readonly IWebHostEnvironment _env;

        public AppSplashController(IAppSplashService splash, ILoggerRepository logger,
            IStorageServices storage, IWebHostEnvironment env)
        {
            _splash = splash;
            _logger = logger;
            _storage = storage;
            _env = env;
        }

        [AllowAnonymous]
        [HttpGet("AppSplash/GetForApp")]
        public async Task<IActionResult> GetForApp()
        {
            try
            {
                var res = await _splash.GetForApp();
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AppSplashController => GetForApp");
                return Response(false, "حدث خطأ");
            }
        }

        [HttpGet("AppSplash/GetAdmin")]
        public async Task<IActionResult> GetAdmin()
        {
            try
            {
                var res = await _splash.GetAdmin();
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AppSplashController => GetAdmin");
                return Response(false, "حدث خطأ");
            }
        }

        [HttpPost("AppSplash/Save")]
        public async Task<IActionResult> Save([FromForm] string? Details, [FromForm] bool IsVisible, IFormFile? Image)
        {
            try
            {
                var current = await _splash.GetAdmin();
                var existing = current.data as AppSplash;

                string imageUrl = existing?.ImageUrl ?? "";
                if (Image != null)
                {
                    var upload = await _storage.UploadImageAsync(Image, _env.WebRootPath);
                    if (!upload.success) return Response(false, upload.msg);
                    imageUrl = Classes.Key.CurrentUrl + @$"\Uplouds\image-{upload.data}";
                }

                if (string.IsNullOrWhiteSpace(imageUrl))
                    return Response(false, "يجب اختيار صورة");

                var res = await _splash.Save(new AppSplash
                {
                    ImageUrl = imageUrl,
                    Details = Details,
                    IsVisible = IsVisible
                });
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AppSplashController => Save");
                return Response(false, "حدث خطأ اثناء الحفظ");
            }
        }
    }
}
