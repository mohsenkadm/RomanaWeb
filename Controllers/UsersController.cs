using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using System.Data;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;
using RomanaWeb.Helper.Repository;
using Microsoft.Net.Http.Headers;
using RomanaWeb.Model;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.EntityMapper;
using AutoMapper;
using RomanaWeb.UploadService;

namespace RomanaWeb.Controllers
{
    public class UsersController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IUsersService _UsersService;
        public readonly IOtpService _otpService;
        private readonly INotificationService _noteService;
        public readonly DB_Context _Context;
        #endregion

        #region Const
        public UsersController(
            ILoggerRepository logger,
            IUsersService UsersService, IOtpService otpService,
             INotificationService noteService,DB_Context dB_Context)
        {
            _logger = logger; 
            _otpService = otpService;

            _UsersService = UsersService; 
            _noteService = noteService;
            _Context= dB_Context;              
        }
        #endregion         

        #region OTP Login (WhatsApp)
        [AllowAnonymous]
        [HttpPost("Users/Login/SendOtp/{Phone}")]
        public async Task<IActionResult> LoginSendOtp(string Phone)
        {
            try
            {
                ResObj res = await _UsersService.LoginSendOtp(Phone);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "UsersController => LoginSendOtp");
                return Response(false, "حدث خطأ اثناء ارسال الكود");
            }
        }

        [AllowAnonymous]
        [HttpPost("Users/Login/VerifyOtp/{Phone},{Code}")]
        public async Task<IActionResult> LoginVerifyOtp(string Phone, string Code)
        {
            try
            {
                ResObj res = await _UsersService.LoginVerifyOtp(Phone, Code);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "UsersController => LoginVerifyOtp");
                return Response(false, "حدث خطأ اثناء التحقق");
            }
        }
        #endregion

        #region LoginUsers (legacy password)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string UserName, string password)
        {
            try
            {
                ResObj res = await _UsersService.Login(UserName, password);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "UsersController => Login => name:" + UserName);
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region RefreshToken
        [AllowAnonymous]
        [HttpPost("Users/RefreshToken/{Id}")]
        public async Task<IActionResult> RefreshToken(int Id)
        {
            try
            {
                ResObj res = await _UsersService.RefreshToken(Id);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PersonController => RefreshToken " + Id);
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region ForgatePassword 
        [AllowAnonymous]
        [HttpPost("Users/ForgatePassword/{Phone}")]
        public async Task<IActionResult> ForgatePassword(string Phone)
        {
            try
            {
                ResObj res = await _UsersService.ForgatePassword(Phone);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PersonController => ForgatePassword => name:" + Phone);
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
        #region Update_Pass_WithCode 
        [AllowAnonymous]
        [HttpPost("Users/Update_Pass_WithCode/{Pass},{Phone},{Code}")]
        public async Task<IActionResult> Update_Pass_WithCode(string Pass, string Phone, string Code)
        {
            try
            {
                ResObj res = await _UsersService.Update_Pass_WithCode(Pass, Phone, Code);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PersonController => Login => name:" + Phone);
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region ConfirmCode  
        [AllowAnonymous]
        [HttpPost("Users/ConfirmCode/{code},{Phone}")]
        public async Task<IActionResult> ConfirmCode(string code, string Phone)
        {
            try
            {
                ResObj res = await _UsersService.ConfirmCode(code, Phone);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PersonController => ConfirmCode");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region SendOTPCodeToPhoneNo 
        [AllowAnonymous]
        [HttpPost("Users/SendOTPCodeToPhoneNo/{Phone}")]
        public async Task<IActionResult> SendOTPCodeToPhoneNo(string Phone)
        {
            try
            {
                ResObj res = await _otpService.SendOTPCodeToPhoneNo(Phone, new Random().Next(1000, 9999).ToString());

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PersonController => SendOTPCodeToPhoneNo => Phone:" + Phone);
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Get Info Users 

        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name)
        {
            try
            {
                ResObj res = await _UsersService.GetAll(Name);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "UsersController => GetAll => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }

        [HttpGet]
        public async Task<FileResult> GetExcelAll(string? Name)
        {
            try
            {
                ResObj res = await _UsersService.GetAll(Name);
                return GenerateExcel("report-users-" + Key.DateTimeIQ + ".xlsx", (List<Users>)res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "UsersController => GetExcelAll");
                return null;
            }
        }

        [NonAction]
        private static FileResult GenerateExcel(string fileName, IEnumerable<Users> users)
        {
            DataTable dataTable = new DataTable("Users");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("اسم الزبون"),
                new DataColumn("رقم الهاتف"),
                new DataColumn("العنوان"),
                new DataColumn("تاكيد"),
                new DataColumn("نشط"),
                new DataColumn("كلمة المرور"),
                new DataColumn("Lat"),
                new DataColumn("Long"),
                new DataColumn("اقرب نقطة دالة"),
                new DataColumn("كود تاكيد"),
            });

            foreach (var u in users)
            {
                dataTable.Rows.Add(
                    u.Name,
                    u.Phone,
                    u.Address,
                    u.IsConfirm == true ? "نعم" : "لا",
                    u.IsActive == true ? "نعم" : "لا",
                    u.Password,
                    u.Lat,
                    u.Long,
                    u.FunctionPoint,
                    u.Code);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using MemoryStream stream = new MemoryStream();
                wb.SaveAs(stream);
                return new FileContentResult(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }
        #endregion                    
        #region insert or update Info Users   admin
        [HttpPost]
        public async Task<IActionResult> PostAdmin(Users Users)
        {
            try
            {        
                                   
                ResObj res;

                if (Users.UserId == 0)
                {                    
                    res = await _UsersService.Post(Users);
                }
                else
                {
                    res = await _UsersService.Update(Users);
                }
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "UsersController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion 
        #region insert or update Info Users 
        [HttpPost]
        public async Task<IActionResult> Post([FromForm]Users Users)
        {
            try
            {
                  
                ResObj res;

                if (Users.UserId == 0)
                {
                    res = await _UsersService.Post(Users);

                  
                }
                else
                {
                    res = await _UsersService.Update(Users);
                }


                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "UsersController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info Users 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _UsersService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "UsersController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get Users ById Info Users 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _UsersService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "UsersController => GetById => name:");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
                                 

    }
}
