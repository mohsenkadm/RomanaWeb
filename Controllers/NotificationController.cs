using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Model;
using Microsoft.Extensions.Hosting;
using RomanaWeb.Models.EntityMapper;
using AutoMapper;
using RomanaWeb.UploadService;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class NotificationController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly INotificationService _NotificationServices;
        DB_Context _Context;
        private readonly IWebHostEnvironment _hostEnvironment;   
        public readonly IStorageServices _storageServices;
        #endregion

        #region Const

        public NotificationController(
            ILoggerRepository logger,
            INotificationService NotificationServices,
            DB_Context dB_Context,
             IWebHostEnvironment hostEnvironment,  IStorageServices storageServices)
        {
            _logger = logger;
            _NotificationServices = NotificationServices;
            _Context=dB_Context;
                _hostEnvironment = hostEnvironment;
            _storageServices = storageServices;
        }
        #endregion

        #region Get Notification
        [AllowAnonymous]
        [HttpGet()]
        public async Task<IActionResult> GetNotificationAll(int? UserId)
        {
            try
            {
                ResObj res = await _NotificationServices.GetNotificationAll(UserId);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PostsController => GetNotificationAll ");
                return Response(false, "حدث خطا");
            }
        }
        #endregion 

        #region Get Notification      
        [HttpGet()]
        public async Task<IActionResult> GetNotificationForRes(int? ResId)
        {
            try
            {
                ResObj res = await _NotificationServices.GetNotificationForRes(ResId);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PostsController => GetNotificationForRes");
                return Response(false, "حدث خطا");
            }
        }
        #endregion

        #region insert  Notification   with image
        [HttpPost]
        public async Task<IActionResult> PostWithImage(Notification Notification)
        {
            try
            {
                if (Notification.FileChoose == null)
                {
                    return Response(false, "رجاءا اختيار ملف التحميل");
                }
                var result = await _storageServices.UploadImageAsync(Notification.FileChoose, _hostEnvironment.WebRootPath);

               
                if (result.success)
                {
                    Notification.Images = Key.CurrentUrl + @$"\Uplouds\image-{result.data}";
                }
                else
                {
                    return Response(false, result);
                }
                ResObj res;                          
                    res = await _NotificationServices.Post(Notification);
                await sendnot(Notification);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "NotificationController => Post  ");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion 
        #region insert  Notification 
        [HttpPost]
        public async Task<IActionResult> Post(Notification Notification)
        {
            try
            {
                Notification.Images = "";
                ResObj res;                          
                    res = await _NotificationServices.Post(Notification);
                await sendnot(Notification);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "NotificationController => Post  ");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion
        private async Task sendnot(Notification notifications)
        {
            try
            {                                       
                await OneSignalSender(notifications.Title, notifications.Details);
            }
            catch (Exception ex)
            { await _logger.WriteAsync(ex, " PostController => sendnot"); }
        }

    }
}
