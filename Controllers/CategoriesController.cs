using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using AutoMapper;
using RomanaWeb.UploadService;
using RomanaWeb.Models.EntityMapper;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class CategoriesController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly ICategoriesService _CategoriesService; 
        private readonly IWebHostEnvironment _hostEnvironment;
        public readonly IMapper _mapper;
        public readonly IStorageServices _storageServices;

        #endregion

        #region Const
        public CategoriesController(
            ILoggerRepository logger,
            ICategoriesService CategoriesService,
             IWebHostEnvironment hostEnvironment, IMapper mapper, IStorageServices storageServices)
        {
            _logger = logger;
            _CategoriesService = CategoriesService;
            _hostEnvironment = hostEnvironment;
            _mapper = mapper;
            _storageServices = storageServices;
        }
        #endregion           

        #region Get Info Categories 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                ResObj res = await _CategoriesService.GetAll();

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CategoriesController => GetAll => name:");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion
                            

        #region insert or update Info Categories 
        [HttpPost]
        public async Task<IActionResult> Post(CategoriesModel CategoriesModel)
        {
            try
            {
                if (CategoriesModel.FileChoose == null)
                {
                    return Response(false, "رجاءا اختيار ملف التحميل");
                }
                var result = await _storageServices.UploadImageAsync(CategoriesModel.FileChoose, _hostEnvironment.WebRootPath);

                Categories Categories = _mapper.Map<Categories>(CategoriesModel);

                if (result.success)
                {
                    Categories.CategoriesImages = Key.CurrentUrl + @$"\Uplouds\image-{result.data}";
                }
                else
                {
                    return Response(false, result);
                }

                ResObj res;
                if (Categories.CategoriesId == 0)
                {
                    res = await _CategoriesService.Post(Categories);
                }
                else
                {
                    res = await _CategoriesService.Update(Categories);
                }
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CategoriesController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info Categories 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _CategoriesService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CategoriesController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get Categories ById Info Categories 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _CategoriesService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CategoriesController => GetById => name:");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion   
    }
}
