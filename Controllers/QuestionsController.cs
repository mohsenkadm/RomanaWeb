using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using RomanaWeb.Models.EntityMapper;
using AutoMapper;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class QuestionsController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IQuestionsService _QuestionsService;
        public readonly IMapper _mapper;
        #endregion

        #region Const
        public QuestionsController(
            ILoggerRepository logger,
            IQuestionsService QuestionsService,
            IMapper mapper)
        {
            _logger = logger;
            _QuestionsService = QuestionsService;
            _mapper = mapper;
        }
        #endregion

        #region Get Info Questions (Public API)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllApp()
        {
            try
            {
                ResObj res = await _QuestionsService.GetAllApp();
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "QuestionsController => GetAllApp");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion   

        #region Get All Questions (Admin)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                ResObj res = await _QuestionsService.GetAll();
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "QuestionsController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region insert or update Info Questions 
        [HttpPost]
        public async Task<IActionResult> Post(QuestionsModel QuestionsModel)
        {
            try
            {
                Questions Questions = _mapper.Map<Questions>(QuestionsModel);

                ResObj res;
                if (Questions.QuestionId == 0)
                {
                    res = await _QuestionsService.Post(Questions);
                }
                else
                {
                    res = await _QuestionsService.Update(Questions);
                }
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "QuestionsController => Post");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info Questions 
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _QuestionsService.Delete(Id);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "QuestionsController => Delete");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get Questions ById 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _QuestionsService.GetById(Id);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "QuestionsController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion   
    }
}