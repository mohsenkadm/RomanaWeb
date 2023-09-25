using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;
using RomanaWeb.Models.Entity;
using RomanaWeb.UploadService;
using Microsoft.Extensions.Hosting;
using AutoMapper;         
using RomanaWeb.Models.EntityMapper;

namespace RomanaWeb.Controllers
{                  
    [Authorize]
    public class ProductsController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IProductsService _ProductsService;   
        public readonly IStorageServices storageServices;      
        private readonly IWebHostEnvironment _hostEnvironment;
        public readonly IMapper _mapper;
        #endregion

        #region Const
        public ProductsController(
            ILoggerRepository logger,
            IProductsService ProductsService,
            IStorageServices storageServices ,IWebHostEnvironment webHostEnvironment, IMapper mapper )
        {
            _logger = logger;
            _ProductsService = ProductsService;         
            this.storageServices= storageServices;
            this._hostEnvironment= webHostEnvironment;  
            this._mapper = mapper;
        }
        #endregion     

        #region Get Info Products  all  
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name, string? RestaurantName, string? SubCategoriesName,int index)
        {
            try
            {
                ResObj res = await _ProductsService.GetAll(Name, RestaurantName, SubCategoriesName, index);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => GetAll => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion  

        #region Get Info Products by RestaurantId and userid   and SubCategoriesId   
        [HttpGet("GetByRestaurantId/{RestaurantId},{UserId},{SubCategoriesId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRestaurantId(int RestaurantId, int UserId, int SubCategoriesId)
        {
            try
            {
                ResObj res = await _ProductsService.GetByRestaurantId(RestaurantId, UserId, SubCategoriesId);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => GetByRestaurantId => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region insert or update Info Products 
        [HttpPost]
        public async Task<IActionResult> Post([FromForm]ProductsModel ProductsModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                     

                    if (ProductsModel.FileChoose == null)
                    {
                        return Response(false, "رجاءا اختيار ملف التحميل");
                    }
                    var result = await storageServices.UploadImageAsync(ProductsModel.FileChoose, _hostEnvironment.WebRootPath);

                    Products Products = _mapper.Map<Products>(ProductsModel);
                    if (result.success)
                    {
                        Products.ProductsImage = Key.CurrentUrl + @$"\Uplouds\image-{result.data}";
                    }
                    else
                    {
                        return Response(false, result);
                    }
                    ResObj res;
                    if (Products.ProductsId == 0)
                    {
                        res = await _ProductsService.Post(Products);
                    }
                    else
                    {
                        res = await _ProductsService.Update(Products);
                    }    
                }
                else
                    return Response(false, "املأ الحقول اولا");
                return Response(true, "تم الحفظ بنجاح");
                
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => Post => name:"  );
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info Products 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                  ResObj res = await _ProductsService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => Delete => name:"  );
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get Products ById Info Products    
        [HttpGet()]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _ProductsService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => GetById => name:"  );
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

       
    }
}
