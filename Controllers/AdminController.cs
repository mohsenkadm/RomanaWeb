using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class AdminController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IAdminServices _AdminServices;
        #endregion

        #region Const
        public AdminController(
            ILoggerRepository logger,
            IAdminServices AdminServices)
        {
            _logger = logger;
            _AdminServices = AdminServices;
        }
        #endregion

        #region changestate users  
        [HttpPost]
        public async Task<IActionResult> changestate(Permission permission)
        {
            try
            {

                ResObj res=  await _AdminServices.changestate(permission);
                
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminController => changestate ");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion 
        #region get permission  
        [HttpGet]
        public async Task<IActionResult> GetPermissionUser(int UserId)
        {
            try
            {
                ResObj res = await _AdminServices.GetPermissionByUserId(UserId);
                 

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {                                                                          
                await _logger.WriteAsync(ex, "AdminController => GetPermissionUser ");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region GetPermissionForLayout
        [HttpGet]
        public async Task<IActionResult> GetPermissionForLayout()
        {
            try
            {
                ResObj res = await _AdminServices.GetPermissionForLayout(UserManager.Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {                                                                    
                await _logger.WriteAsync(ex, "AdminController => GetPermission ");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");           
            }
        }
        #endregion

        #region LoginUser 
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string AdminNo, string password)
        {
            try
            {
                ResObj res = await _AdminServices.Login(AdminNo, password);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminController => Login => name:" + AdminNo);
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
           
        #region Get Info Admin with search
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name)
        {
            try
            {
                ResObj res = await _AdminServices.GetAll(Name);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminController => GetAdminAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion  
                     
        #region insert or update Info Admin 
        [HttpPost]
        public async Task<IActionResult> Post(Admin Admin)
        {
            try
            {
                ResObj res;
                if (Admin.AdminId == 0)
                    res = await _AdminServices.Post(Admin);
                else
                    res = await _AdminServices.Update(Admin);        
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminController => Post  ");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info Admin 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _AdminServices.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminController => Delete");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get Admin ById Info Admin 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _AdminServices.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
        #region logout
        [HttpGet]     
        public async Task<IActionResult> Logout()
        {
            try
            {                                                                                                 
                return Response(true, "تم تسجيل الخروج بنجاح");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "AdminController => Logout");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
    }
}
