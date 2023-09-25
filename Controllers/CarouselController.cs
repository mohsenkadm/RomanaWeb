using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;
using RomanaWeb.Models.EntityMapper;
using AutoMapper;
using RomanaWeb.UploadService;

namespace RomanaWeb.Controllers
{                       
    [Authorize]                            
    public class CarouselController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly ICarouselService _CarouselService;
         private readonly IWebHostEnvironment _hostEnvironment; 
        public readonly IMapper _mapper;
        public readonly IStorageServices _storageServices;
        #endregion

        #region Const
        public CarouselController(
            ILoggerRepository logger,
            ICarouselService CarouselService,
             IWebHostEnvironment hostEnvironment,IMapper mapper, IStorageServices storageServices)
        {
            _logger = logger;
            _CarouselService = CarouselService;
            _hostEnvironment = hostEnvironment;
            _mapper = mapper;
            _storageServices = storageServices;
        }
        #endregion

        #region Get Info Carousel 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllApp()
        {
            try
            {
                ResObj res = await _CarouselService.GetAllApp();

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CarouselController => GetAllApp");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion   

        #region Get Info Carousel 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                ResObj res = await _CarouselService.GetAll();

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CarouselController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion
                           

        #region insert or update Info Carousel 
        [HttpPost]
        public async Task<IActionResult> Post(CarouselModel CarouselModel)
        {
            try
            {
                if (CarouselModel.FileChoose == null )
                {
                    return Response(false, "رجاءا اختيار ملف التحميل");
                }
                var result = await _storageServices.UploadImageAsync(CarouselModel.FileChoose, _hostEnvironment.WebRootPath);

                Carousel Carousel = _mapper.Map<Carousel>(CarouselModel);

                if (result.success)
                {
                    Carousel.Image = Key.CurrentUrl + @$"\Uplouds\image-{result.data}";
                }
                else
                {
                    return Response(false, result);
                }

                ResObj res; 
                if (Carousel.CarouseId == 0)
                { 
                    res = await _CarouselService.Post(Carousel);  
                }
                else
                {
                    res = await _CarouselService.Update(Carousel);
                }
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CarouselController => Post => name:"  );
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info Carousel 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _CarouselService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CarouselController => Delete => name:"  );
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get Carousel ById Info Carousel 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _CarouselService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CarouselController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion   
    }
}
