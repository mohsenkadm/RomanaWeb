using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;
using RomanaWeb.Helper.Interface;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RomanaWeb.Controllers
{                   
    [Authorize]
    public class StarsController : MasterController
    {
        #region service
        public readonly IStarsService service;
        public readonly ILoggerRepository _logger;
        #endregion

        #region Cons
        public StarsController(IStarsService starsService,
            ILoggerRepository logger )
        {
            _logger = logger;
            service = starsService;
        }
        #endregion           

        #region GetAll
        // GET api/<StarsController>/5
        [HttpGet]
        public async Task<IActionResult> GetAll(string ? RestaurantName,int index)
        {
            try
            {
                var item = await service.GetAll(RestaurantName, index);
                return Response(true, item.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "StarsController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب المعلومات");
            }
        }
        #endregion 

        #region GetByRestaurantId
        // GET api/<StarsController>/5
        [HttpGet("GetByRestaurantId/{RestaurantId}")]
        public async Task<IActionResult> GetByRestaurantId(int RestaurantId)
        {
            try
            {
                var item = await service.GetByRestaurantId(RestaurantId);
                return Response(true, item.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "StarsController => GetByRestaurantId");
                return Response(false, "حدث خطأ اثناء عملية جلب المعلومات");
            }
        }
        #endregion   
                       

        #region Get
        // GET api/<StarsController>/5
        [HttpGet()]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var item= await service.GetById(id);
                return Response(true, item.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "StarsController => Get");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }
        #endregion

        #region Post
        // POST api/<StarsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Stars stars)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await service.Post(stars);
                    return Response(true, "تم الحفظ بنجاح");
                }
                else
                    return Response(true, "املأ الحقول اولا");
            }
            catch(Exception ex)
            {                          
                await _logger.WriteAsync(ex, "StarsController => Post");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }                  
        #endregion

        #region Put
        // PUT api/<StarsController>/5
        [HttpPut]
        public async Task<IActionResult> Put([FromBody]Stars stars)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await service.Post(stars);
                    return Response(true, "تم التعديل بنجاح");
                }
                else
                    return Response(true, "املأ الحقول اولا");
            }
            catch(Exception ex)
            {                          
                await _logger.WriteAsync(ex, "StarsController => Put");
                return Response(false, "حدث خطأ اثناء عملية التعديل");
            }
        }
        #endregion

        #region Delete
        // DELETE api/<StarsController>/5
        [HttpDelete()]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await service.Delete(id);
                return Response(true,"تم الحذف بنجاح");  
            }
            catch(Exception ex)
            {

                await _logger.WriteAsync(ex, "StarsController => Delete");
                return Response(false, "حدث خطأ اثناء عملية الحذف");
            }
        }
        #endregion
    }
}
