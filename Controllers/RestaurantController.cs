using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
using System.Data;

namespace RomanaWeb.Controllers
{
    public class RestaurantController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IRestaurantService _RestaurantService;
        private readonly IWebHostEnvironment _hostEnvironment;
        public readonly IMapper _mapper;
        public readonly IStorageServices _storageServices;     
        public readonly DB_Context _Context;
        private readonly IConfiguration _config;
        #endregion

        #region Const
        public RestaurantController(
            ILoggerRepository logger,
            IRestaurantService RestaurantService,
             IWebHostEnvironment hostEnvironment,   DB_Context dB_Context, IMapper mapper, IStorageServices storageServices,
             IConfiguration config)
        {
            _logger = logger;
            _RestaurantService = RestaurantService;      
            _Context= dB_Context;
            _hostEnvironment = hostEnvironment;
            _mapper = mapper;
            _storageServices = storageServices;
            _config = config;
        }
        #endregion         

        #region LoginRestaurant 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string UserName, string password)
        {
            try
            {
                ResObj res = await _RestaurantService.Login(UserName, password);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => Login => name:" + UserName);
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region GetAllForApp Info Restaurant (DEPRECATED - Section 1.2 / 9)
        // Deprecated: use GetByUserLocation. Kept behind feature flag Stores:AllowFetchByCity.
        [AllowAnonymous]
        [Obsolete("Use GetByUserLocation. Section 1.2.")]
        [HttpGet("GetAllForApp/{Name},{CategoriesId},{Long},{Lat},{CityId}")]
        public async Task<IActionResult> GetAllForApp(string? Name, int CategoriesId, double Long, double Lat,int? CityId=0)
        {
            try
            {
                if (!_config.GetValue<bool>("Stores:AllowFetchByCity", false))
                    return StatusCode(410, new { success = false, msg = "This endpoint is deprecated. Use /Restaurant/GetByUserLocation." });

#pragma warning disable CS0618
                ResObj res = await _RestaurantService.GetAllForApp(Name, CategoriesId,Long,Lat, CityId);
#pragma warning restore CS0618

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => GetAllForApp => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region GetByUserLocation (Section 1.2)
        [AllowAnonymous]
        [HttpGet("Restaurant/GetByUserLocation")]
        public async Task<IActionResult> GetByUserLocation([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double? radius_km = null)
        {
            try
            {
                ResObj res = await _RestaurantService.GetByUserLocation(lat, lng, radius_km);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => GetByUserLocation");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion
        #region GetAllForApp Info Restaurant              
        [HttpGet("GetCountForRes/{Id},{datefrom},{dateto}")]
        public async Task<IActionResult>  GetCountForRes(int Id, DateTime datefrom, DateTime dateto)
        {
            try
            {
                ResObj res = await _RestaurantService.GetCountForRes(Id, datefrom,dateto);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => GetCountForRes => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion     
        #region Get Report Info Restaurant              
        [HttpGet()]
        public async Task<IActionResult> GetReportRes(string RestaurantName, DateTime datefrom, DateTime dateto)
        {
            try
            {
                ResObj res = await _RestaurantService.GetReportRes(RestaurantName, datefrom,dateto);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => GetCountForRes => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion   

        #region GetTopAllForApp Info Restaurant (DEPRECATED - Section 1.2 / 9)
        [AllowAnonymous]
        [Obsolete("Use GetByUserLocation. Section 1.2.")]
        [HttpGet("GetTopAllForApp/{Long},{Lat},{CityId}")]
        public async Task<IActionResult> GetTopAllForApp(double Long, double Lat,int? CityId=0)
        {
            try
            {
                if (!_config.GetValue<bool>("Stores:AllowFetchByCity", false))
                    return StatusCode(410, new { success = false, msg = "This endpoint is deprecated. Use /Restaurant/GetByUserLocation." });

#pragma warning disable CS0618
                ResObj res = await _RestaurantService.GetTopAllForApp(Long,Lat, CityId);
#pragma warning restore CS0618

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => GetTopAllForApp => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region Get Info Restaurant 
         
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name)
        {
            try
            {
                ResObj res = await _RestaurantService.GetAll(Name);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => GetAll => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }

        [HttpGet]
        public async Task<FileResult> GetExcelAll(string? Name)
        {
            try
            {
                ResObj res = await _RestaurantService.GetAll(Name);
                return GenerateExcel("report-restaurant-" + Key.DateTimeIQ + ".xlsx", (List<Restaurant>)res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => GetExcelAll");
                return null;
            }
        }

        [NonAction]
        private static FileResult GenerateExcel(string fileName, IEnumerable<Restaurant> restaurants)
        {
            DataTable dataTable = new DataTable("Restaurant");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("الاسم"),
                new DataColumn("التفاصيل"),
                new DataColumn("القسم"),
                new DataColumn("خلفية"),
                new DataColumn("لوكو"),
                new DataColumn("العنوان"),
                new DataColumn("تقييم"),
                new DataColumn("مفتوح"),
                new DataColumn("نشط"),
                new DataColumn("مفضلة"),
                new DataColumn("اسم المستخدم"),
                new DataColumn("كلمة المرور"),
                new DataColumn("Long"),
                new DataColumn("Lat"),
                new DataColumn("اقل سعر"),
                new DataColumn("اسم المنطقة"),
                new DataColumn("كود"),
            });

            foreach (var r in restaurants)
            {
                dataTable.Rows.Add(
                    r.Name,
                    r.Details,
                    r.CategoriesName,
                    r.Background,
                    r.Logo,
                    r.Address,
                    r.IsStars == true ? "نعم" : "لا",
                    r.IsClosed == true ? "لا" : "نعم",
                    r.IsActive == true ? "نعم" : "لا",
                    r.IsTop == true ? "نعم" : "لا",
                    r.UserName,
                    r.Password,
                    r.Long,
                    r.Lat,
                    r.MinimumPrice,
                    r.Areaname,
                    r.Code);
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


        #region Set Restaurant SetIsColsed
        [HttpPost("Restaurant/SetIsColsed/{Id}/{Closed}")]
        public async Task<IActionResult> SetIsColsed(int Id,bool Closed)
        {
            try
            {
                ResObj res = await _RestaurantService.SetIsColsed(Id,Closed);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }                                            
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => SetIsColsed");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Set Restaurant SetIsStars (Section 1.3 - admin only)
        [HttpPost("Restaurant/SetIsStars/{Id}/{Stars}")]
        public async Task<IActionResult> SetIsStars(int Id, bool Stars)
        {
            try
            {
                if (UserManager == null || !string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return Response(false, "غير مصرح، تفعيل التقييم متاح للأدمن فقط");

                ResObj res = await _RestaurantService.SetIsStars(Id, Stars);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => SetIsStars");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region GetResNotApproveAll     
        public async Task<IActionResult> GetResNotApproveAll()
        {
            try
            {
                var item = await _RestaurantService.GetResNotApproveAll();
                return Response(true, item.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PersonController => GetResNotApproveAll");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }
        #endregion    
        #region Set Restaurant SetIsStars         
        public async Task<IActionResult> SetIsApproved(int Id)
        {
            try
            {
                ResObj res = await _RestaurantService.SetIsApproved(Id);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => SetIsApproved");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion     

        #region insert or update Info Restaurant   admin
        [HttpPost]
        public async Task<IActionResult> PostAdmin(RestaurantModel RestaurantModel)
        {
            try
            {
               
                Restaurant Restaurant = _mapper.Map<Restaurant>(RestaurantModel);
                if (Restaurant.RestaurantId == 0)
                {
                    if (RestaurantModel.Logo == null || RestaurantModel.Background == null)
                    {
                        return Response(false, "رجاءا اختيار ملف التحميل");
                    }
                }

                if (RestaurantModel.Logo != null)
                {
                    var result = await _storageServices.UploadImageAsync(RestaurantModel.Logo, _hostEnvironment.WebRootPath);
                    if (result.success)
                    {
                        Restaurant.Logo = Key.CurrentUrl + @$"\Uplouds\image-{result.data}";
                    }
                    else
                    {
                        return Response(false, result);
                    }
                } 
                if (RestaurantModel.Background != null)
                {
                    var result = await _storageServices.UploadImageAsync(RestaurantModel.Background, _hostEnvironment.WebRootPath);
                    if (result.success)
                    {
                        Restaurant.Background = Key.CurrentUrl + @$"\Uplouds\image-{result.data}";
                    }
                    else
                    {
                        return Response(false, result);
                    }
                }                         
                ResObj res;

                if (Restaurant.RestaurantId == 0)
                {
                    Restaurant.Code = "";
                    res = await _RestaurantService.Post(Restaurant, autoApprove: true);
                }
                else
                {
                    // Section 1.1/1.3: admin path may change CostDelivery & IsStars.
                    res = await _RestaurantService.UpdateAdmin(Restaurant);
                }
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion 
        #region insert or update Info Restaurant 
        [HttpPost]
        public async Task<IActionResult> Post([FromForm]RestaurantModel RestaurantModel)
        {
            try
            {
                //CodeRes code=new CodeRes();
                //if (RestaurantModel.RestaurantId > 0)
                //{
                //    ResObj checkcode = await _RestaurantService.CheckCode(RestaurantModel.Code);
                //    if (checkcode.success == false)
                //    {
                //        return Response(false, checkcode.msg);
                //    }                  
                //     code = (CodeRes)checkcode.data;
                //}
                Restaurant Restaurant = _mapper.Map<Restaurant>(RestaurantModel);
                if (Restaurant.RestaurantId == 0)
                {
                    if (RestaurantModel.Logo == null || RestaurantModel.Background == null)
                    {
                        return Response(false, "رجاءا اختيار ملف التحميل");
                    }
                }

                if (RestaurantModel.Logo != null)
                {
                    var result = await _storageServices.UploadImageAsync(RestaurantModel.Logo, _hostEnvironment.WebRootPath);
                    if (result.success)
                    {
                        Restaurant.Logo = Key.CurrentUrl + @$"\Uplouds\image-{result.data}";
                    }
                    else
                    {
                        return Response(false, result);
                    }
                } 
                if (RestaurantModel.Background != null)
                {
                    var result = await _storageServices.UploadImageAsync(RestaurantModel.Background, _hostEnvironment.WebRootPath);
                    if (result.success)
                    {
                        Restaurant.Background = Key.CurrentUrl + @$"\Uplouds\image-{result.data}";
                    }
                    else
                    {
                        return Response(false, result);
                    }
                }
                //if (RestaurantModel.RestaurantId > 0)
                //    Restaurant.Code = code.Code; 
                ResObj res;

                if (Restaurant.RestaurantId == 0)
                {
                    res = await _RestaurantService.Post(Restaurant, autoApprove: false);

                   // code.IsFree = false;
                  //  await _RestaurantService.UpdateCode(code);
                }
                else
                {
                    res = await _RestaurantService.Update(Restaurant);
                }


                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info Restaurant 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _RestaurantService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get Restaurant ById Info Restaurant 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _RestaurantService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => GetById => name:");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region UpdateLocationInfo
        // GET api/<RestaurantController>/
        [HttpPost("Restaurant/UpdateLocationInfo/{Id},{Long},{Lat}")]
        public async Task<IActionResult> UpdateLocationInfo(int Id, double Long, double Lat)
        {
            try
            {
                var item = await _RestaurantService.UpdateLocationInfo(Id, Long, Lat);
                return Response(true, item.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => UpdateLocationInfo");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }
        #endregion

        #region Set Restaurant SetInsta
        [HttpPost("Restaurant/SetInsta/{Id}/{Url}")]
        public async Task<IActionResult> SetInsta(int Id, string Url)
        {
            try
            {
                ResObj res = await _RestaurantService.SetInsta(Id, Url);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => SetInsta");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region RefreshToken
        [AllowAnonymous]
        [HttpPost("Restaurant/RefreshToken/{Id}")]
        public async Task<IActionResult> RefreshToken(int Id)
        {
            try
            {
                ResObj res = await _RestaurantService.RefreshToken(Id);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => RefreshToken " + Id);
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion 
    }
}
