using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
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
    [Authorize]
    public class SaleManController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly ISaleManService _SaleManService;
        private readonly IWebHostEnvironment _hostEnvironment;
        public readonly IMapper _mapper;
        public readonly IStorageServices _storageServices;   
        private readonly INotificationService _noteService;
        public readonly DB_Context _Context;
        #endregion

        #region Const
        public SaleManController(
            ILoggerRepository logger,
            ISaleManService SaleManService,
             IWebHostEnvironment hostEnvironment,
             INotificationService noteService,DB_Context dB_Context, IMapper mapper, IStorageServices storageServices)
        {
            _logger = logger;
            _SaleManService = SaleManService; 
            _noteService = noteService;
            _Context= dB_Context;
            _hostEnvironment = hostEnvironment;
            _mapper = mapper;
            _storageServices = storageServices;
        }
        #endregion

        // Admin role guard. SaleMan management (add/edit/remove) is admin-only;
        // restaurants no longer manage their own salesmen.
        [NonAction]
        private bool IsAdmin() =>
            string.Equals(UserManager?.Role, "admin", StringComparison.OrdinalIgnoreCase);

        #region LoginSaleMan 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string Phone, string password)
        {
            try
            {
                ResObj res = await _SaleManService.Login(Phone, password);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManController => Login => name:" + Phone);
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
                                         
        #region GetCountForSale Info SaleMan              
        [HttpGet("GetCountForSale/{Id},{datefrom},{dateto}")]
        public async Task<IActionResult>  GetCountForSale(int Id, DateTime datefrom, DateTime dateto)
        {
            try
            {
                ResObj res = await _SaleManService.GetCountForSale(Id, datefrom,dateto);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManController => GetCountForSale => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Get Info SaleMan 

        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name)
        {
            try
            {
                ResObj res = await _SaleManService.GetAll(Name);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManController => GetAll => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion

                                    
                                                 
        #region insert or update Info SaleMan (admin only)
        [HttpPost]
        public async Task<IActionResult> Post([FromForm]SaleManModel SaleManModel)
        {
            try
            {
                if (!IsAdmin())
                    return Response(false, "هذه العملية مخصصة لمدير التطبيق فقط");

                SaleMan SaleMan = _mapper.Map<SaleMan>(SaleManModel);

                ResObj res;

                if (SaleMan.SaleManId == 0)
                {
                    res = await _SaleManService.Post(SaleMan);

                }
                else
                {
                    res = await _SaleManService.Update(SaleMan);
                }


                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info SaleMan (admin only)
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                if (!IsAdmin())
                    return Response(false, "هذه العملية مخصصة لمدير التطبيق فقط");
                ResObj res = await _SaleManService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get SaleMan ById Info SaleMan 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _SaleManService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManController => GetById => name:");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        // Section 6 - working/stopped toggle (admin or driver themselves).
        // POST /SaleMan/SetAvailability?Id=12&isAvailable=false
        // The dispatcher (DriverDispatchService) excludes drivers with IsAvailable=false.
        #region SetAvailability
        [HttpPost("SaleMan/SetAvailability")]
        public async Task<IActionResult> SetAvailability(int Id, bool isAvailable)
        {
            try
            {
                if (Id <= 0) return Response(false, "معرف غير صالح");
                ResObj res = await _SaleManService.SetAvailability(Id, isAvailable);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManController => SetAvailability => Id:" + Id);
                return Response(false, "حدث خطأ اثناء تغيير الحالة");
            }
        }

        // Convenience: a driver toggles their own availability from the mobile app.
        // POST /SaleMan/ToggleMyAvailability?isAvailable=false
        [HttpPost("SaleMan/ToggleMyAvailability")]
        public async Task<IActionResult> ToggleMyAvailability(bool isAvailable)
        {
            try
            {
                int driverId = UserManager?.Id ?? 0;
                if (driverId <= 0) return Response(false, "غير مصرح");
                ResObj res = await _SaleManService.SetAvailability(driverId, isAvailable);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManController => ToggleMyAvailability");
                return Response(false, "حدث خطأ اثناء تغيير الحالة");
            }
        }
        #endregion

        [HttpGet("drivers/me/zones")]
        public async Task<IActionResult> GetMyZones()
        {
            try
            {
                int driverId = UserManager?.Id ?? 0;
                if (driverId <= 0) return Response(false, "غير مصرح");
                var ids = await _Context.SaleManZone.AsNoTracking()
                    .Where(sz => sz.SaleManId == driverId)
                    .Select(sz => sz.ZoneId)
                    .ToListAsync();
                var names = await _Context.Zone.AsNoTracking()
                    .Where(z => ids.Contains(z.ZoneId))
                    .Select(z => new { z.ZoneId, z.Name })
                    .ToListAsync();
                return Response(true, names);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManController => GetMyZones");
                return Response(false, "خطأ");
            }
        }

    }
}
