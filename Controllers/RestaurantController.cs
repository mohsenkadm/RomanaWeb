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
    public class RestaurantController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IRestaurantService _RestaurantService;
        private readonly IWebHostEnvironment _hostEnvironment;
        public readonly IMapper _mapper;
        public readonly IStorageServices _storageServices;   
        private readonly INotificationService _noteService;
        public readonly DB_Context _Context;
        #endregion

        #region Const
        public RestaurantController(
            ILoggerRepository logger,
            IRestaurantService RestaurantService,
             IWebHostEnvironment hostEnvironment,
             INotificationService noteService,DB_Context dB_Context, IMapper mapper, IStorageServices storageServices)
        {
            _logger = logger;
            _RestaurantService = RestaurantService; 
            _noteService = noteService;
            _Context= dB_Context;
            _hostEnvironment = hostEnvironment;
            _mapper = mapper;
            _storageServices = storageServices;
        }
        #endregion         

        #region LoginRestaurant 
        [AllowAnonymous]
        [HttpPost]
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


        #region GetAllForApp Info Restaurant 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllForApp(string? Name,int UserId,int CategoriesId)
        {
            try
            {
                ResObj res = await _RestaurantService.GetAllForApp(Name, UserId, CategoriesId);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RestaurantController => GetAllForApp => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion   

        #region GetAllForApp Info Restaurant 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetTopAllForApp(int? UserId,double Long,double Lat,int index)
        {
            try
            {
                ResObj res = await _RestaurantService.GetTopAllForApp(UserId,Long,Lat, index);

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
        #endregion
                                                         

        #region Set Order SetIsColsed
        [HttpPost]
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
                    res = await _RestaurantService.Post(Restaurant);
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
        #region insert or update Info Restaurant 
        [HttpPost]
        public async Task<IActionResult> Post(RestaurantModel RestaurantModel)
        {
            try
            {
                CodeRes code=new CodeRes();
                if (RestaurantModel.RestaurantId > 0)
                {
                    ResObj checkcode = await _RestaurantService.CheckCode(RestaurantModel.Code);
                    if (checkcode.success == false)
                    {
                        return Response(false, checkcode.msg);
                    }

                     code = (CodeRes)checkcode.data;
                }
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
                if (RestaurantModel.RestaurantId > 0)
                    Restaurant.Code = code.Code; 
                ResObj res;

                if (Restaurant.RestaurantId == 0)
                {
                    res = await _RestaurantService.Post(Restaurant);

                    code.IsFree = false;
                    await _RestaurantService.UpdateCode(code);
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
        [HttpPost("UpdateLocationInfo/{Id},{Long},{Lat}")]
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


    }
}
