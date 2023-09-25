using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;
using RomanaWeb.Models.EntityMapper;
using RomanaWeb.UploadService;

namespace RomanaWeb.Controllers
{
    public class CodeResController : MasterController
    {

        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly ICodeResService _CodeResService;
        #endregion

        #region Const
        public CodeResController(
            ILoggerRepository logger,
            ICodeResService CodeResService)
        {
            _logger = logger;
            _CodeResService = CodeResService;   
        }
        #endregion
                             

        #region Get Info CodeRes  
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                ResObj res = await _CodeResService.GetAll();

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CodeResController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region insert or update Info CodeRes 
        [HttpPost]
        public async Task<IActionResult> Post(CodeRes CodeRes)
        {
            try
            {         

                ResObj res;          
                    res = await _CodeResService.Post(CodeRes);  
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CodeResController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info CodeRes 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _CodeResService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CodeResController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get CodeRes ById Info CodeRes 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _CodeResService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CodeResController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion   
    }
}
