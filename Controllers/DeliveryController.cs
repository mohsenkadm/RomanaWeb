using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class DeliveryController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IDeliveryService _DeliveryService;
        #endregion

        #region Const
        public DeliveryController(
            ILoggerRepository logger,
            IDeliveryService DeliveryService)
        {
            _logger = logger;
            _DeliveryService = DeliveryService;
        }
        #endregion


        #region Get Orders By No and RestaurantId 
        [HttpGet("GetDeliveryByNoAndRestaurantId/{No},{RestaurantId}")]
        public async Task<IActionResult> GetDeliveryByNoAndRestaurantId(string? No, int? RestaurantId)
        {
            try
            {
                ResObj res = await _DeliveryService.GetDeliveryByNoAndRestaurantId(No, RestaurantId);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetDeliveryByNoAndRestaurantId");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Get Info Delivery   
        [HttpGet]
        public async Task<IActionResult> GetAll(string? Name,int? CountriesId,string? RestaurantName
            , DateTime datefrom, DateTime dateto, string? No)
        {
            try
            {
                ResObj res = await _DeliveryService.GetAll(Name, CountriesId, RestaurantName, datefrom, dateto, No);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DeliveryController => GetAll");
                return Response(false, "حدث خطأ اثناء عملية جلب البيانات");
            }
        }
        #endregion  
 
        #region insert or update Info Delivery 
        [HttpPost]    
        public async Task<IActionResult> Post([FromBody]Delivery Delivery)
        {
            try
            {
                ResObj res;
                res = await _DeliveryService.Post(Delivery);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DeliveryController => Post => name:");
                return Response(false, "حدث خطا اثناء عملية الحفظ");
            }
        }
        #endregion

        #region delete Info Delivery 
        [HttpDelete]   
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                ResObj res = await _DeliveryService.Delete(Id);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DeliveryController => Delete => name:");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get Delivery ById Info Delivery 
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                ResObj res = await _DeliveryService.GetById(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DeliveryController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion 
    }
}
