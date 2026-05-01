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
using RomanaWeb.Helper.Repository;

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
        #region Get Info Products  all  
        [HttpGet("Products/GetAllBySearch/{Name},{index}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBySearch(string? Name, int index)
        { 
            try
            {
                ResObj res = await _ProductsService.GetAllBySearch(Name,  index);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => GetAllBySearch => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion                                                                                                                                                                             

        #region Get Info Products by RestaurantId and userid   and SubCategoriesId   
        [HttpGet("Products/GetByRestaurantId/{RestaurantId},{SubCategoriesId},{prodname}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRestaurantId(int RestaurantId, int SubCategoriesId,string? prodname)
        {
            try
            {
                ResObj res = await _ProductsService.GetByRestaurantId(RestaurantId, SubCategoriesId, prodname);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => GetByRestaurantId => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion



        #region Set Products SetIsFree
        [HttpPost("Products/SetIsFree/{Id}/{Free}")]
        public async Task<IActionResult> SetIsFree(int Id, bool Free)
        {
            try
            {
                ResObj res = await _ProductsService.SetIsFree(Id, Free);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => SetIsFree");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        //#region insert or update Info Products 
        //[HttpPost]
        //public async Task<IActionResult> Post([FromForm]ProductsModel ProductsModel)
        //{
        //    try
        //    {
        //        //if (ModelState.IsValid)
        //        //{
        //        if (ProductsModel.ProductsId == 0)
        //        {
        //            if (ProductsModel.FileChoose == null)
        //            {
        //                return Response(false, "رجاءا اختيار ملف التحميل");
        //            }
        //        }
        //        ResObj result=new ResObj();
        //        if (ProductsModel.FileChoose != null)
        //             result = await storageServices.UploadImageAsync(ProductsModel.FileChoose, _hostEnvironment.WebRootPath);

        //        Products Products = _mapper.Map<Products>(ProductsModel);
        //        if (ProductsModel.FileChoose != null)
        //        {
        //            if (result.success)
        //            {
        //                Products.ProductsImage = Key.CurrentUrl + @$"\Uplouds\image-{result.data}";
        //            }
        //            else
        //            {
        //                return Response(false, result);
        //            }
        //        }
        //            ResObj res;
        //            if (Products.ProductsId == 0)
        //            {
        //                res = await _ProductsService.Post(Products);
        //            }
        //            else
        //            {
        //                res = await _ProductsService.Update(Products);
        //            }    
        //       // }
        //       // else
        //       //     return Response(false, "املأ الحقول اولا");
        //        return Response(true, "تم الحفظ بنجاح");
                
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.WriteAsync(ex, "ProductsController => Post => name:"  );
        //        return Response(false, "حدث خطا اثناء عملية الحفظ");
        //    }
        //}
        //#endregion
              
        #region insert or update Info Products     2
        [HttpPost]
        public async Task<IActionResult> Post([FromForm]ProductsModel ProductsModel)
        {
            try
            {       
                if (ProductsModel.ProductsId == 0)
                {
                    if (ProductsModel.FileChoose.Count() == 0)
                    {
                        return Response(false, "رجاءا اختيار ملف التحميل");
                    }
                }                              
                Products Products = _mapper.Map<Products>(ProductsModel);
                ResObj res;
                if (Products.ProductsId == 0)
                {
                    res = await _ProductsService.Post(Products);
                }
                else
                {
                    res = await _ProductsService.Update(Products);
                }
                if(ProductsModel.FileChoose!=null)
                foreach (var item in ProductsModel.FileChoose)
                {
                    if (item != null)
                    {

                        var result = await storageServices.UploadImageAsync(item, _hostEnvironment.WebRootPath);
                        if (result.success)
                        {
                            await _ProductsService.PostImages(new Images()
                            {
                                ProductsId = Convert.ToInt32(res.data),
                                ImagePath =Key.CurrentUrl + @$"\Uplouds\image-{result.data}"     
                            });
                        }
                        else
                        {
                            return Response(false, result);
                        }
                    }
                }
                                                 
                return Response(true, "تم الحفظ بنجاح");
                
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => Post => name:"  );
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region Get Info GetImagesByProductsId 
        [AllowAnonymous]
        [HttpGet("GetImagesByProductsId/{Id}")]
        public async Task<IActionResult> GetImagesByProductsId(int Id)
        {
            try
            {
                ResObj res = await _ProductsService.GetImagesByProductsId(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "PostsController => GetImagesByPostId ");
                return Response(false, "حدث خطا");
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
        [HttpGet("Products/GetById/{Id}")]
        [AllowAnonymous]
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

        #region DeleteImage Info Products 
        [HttpDelete]
        public async Task<IActionResult> DeleteImage(int Id)
        {
            try
            {
                ResObj res = await _ProductsService.DeleteImage(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => DeleteImage => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Product Sizes

        [HttpGet("Products/GetSizesByProductId/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSizesByProductId(int productId)
        {
            try
            {
                ResObj res = await _ProductsService.GetSizesByProductId(productId);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => GetSizesByProductId");
                return Response(false, "حدث خطأ اثناء جلب الأحجام");
            }
        }

        [HttpPost("Products/PostSize")]
        public async Task<IActionResult> PostSize([FromBody] ProductSize size)
        {
            try
            {
                if (size == null || size.ProductsId <= 0 || string.IsNullOrWhiteSpace(size.SizeName))
                    return Response(false, "بيانات الحجم غير صحيحة");
                ResObj res = await _ProductsService.PostSize(size);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => PostSize");
                return Response(false, "حدث خطأ اثناء حفظ الحجم");
            }
        }

        [HttpDelete("Products/DeleteSize/{sizeId}")]
        public async Task<IActionResult> DeleteSize(int sizeId)
        {
            try
            {
                ResObj res = await _ProductsService.DeleteSize(sizeId);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => DeleteSize");
                return Response(false, "حدث خطأ اثناء حذف الحجم");
            }
        }

        #endregion

        #region Product Ingredients

        [HttpGet("Products/GetIngredientsByProductId/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetIngredientsByProductId(int productId)
        {
            try
            {
                ResObj res = await _ProductsService.GetIngredientsByProductId(productId);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => GetIngredientsByProductId");
                return Response(false, "حدث خطأ اثناء جلب المكونات");
            }
        }

        [HttpPost("Products/PostIngredient")]
        public async Task<IActionResult> PostIngredient([FromBody] ProductIngredient ingredient)
        {
            try
            {
                if (ingredient == null || ingredient.ProductsId <= 0 || string.IsNullOrWhiteSpace(ingredient.IngredientName))
                    return Response(false, "بيانات المكون غير صحيحة");
                ResObj res = await _ProductsService.PostIngredient(ingredient);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => PostIngredient");
                return Response(false, "حدث خطأ اثناء حفظ المكون");
            }
        }

        [HttpDelete("Products/DeleteIngredient/{ingredientId}")]
        public async Task<IActionResult> DeleteIngredient(int ingredientId)
        {
            try
            {
                ResObj res = await _ProductsService.DeleteIngredient(ingredientId);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProductsController => DeleteIngredient");
                return Response(false, "حدث خطأ اثناء حذف المكون");
            }
        }

        #endregion

    }
}
