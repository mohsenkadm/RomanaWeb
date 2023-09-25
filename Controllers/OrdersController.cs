using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using RomanaWeb.Helper.Interface;
using AutoMapper;
using RomanaWeb.Models.EntityMapper;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Repository;    

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class OrdersController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IOrdersService _OrdersService;
        private readonly INotificationService _noteService;    
        public readonly IMapper _mapper;
        #endregion

        #region Const
        public OrdersController(
            ILoggerRepository logger,
            IOrdersService OrdersService,
            INotificationService noteService, IMapper mapper)
        {
            _logger = logger;
            _OrdersService = OrdersService;
            _noteService = noteService;
            _mapper = mapper;           
        }
        #endregion

        #region Get Info order with detail     
        [HttpGet]
        public async Task<IActionResult> GetOrdersWithDetailAll(int Id)
        {
            try
            {
                ResObj res = await _OrdersService.GetOrdersWithDetailAll(Id);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetOrdersWithDetailAll => name:");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Get Orders By OrderNo and UserId 
        [HttpGet("GetOrdersByOrderNoAndUserId/{OrderNo},{UserId}")]
        public async Task<IActionResult> GetOrdersByOrderNoAndUserId(string? OrderNo, int? PersonId)
        {
            try
            {
                ResObj res = await _OrdersService.GetOrdersByOrderNoAndUserId(OrderNo, PersonId);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetOrdersByOrderNoAndUserId");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion    

        #region Get Orders By OrderNo and RestaurantId 
        [HttpGet("GetOrdersByOrderNoAndRestaurantId/{OrderNo},{RestaurantId}")]
        public async Task<IActionResult> GetOrdersByOrderNoAndRestaurantId(string? OrderNo, int? RestaurantId)
        {
            try
            {
                ResObj res = await _OrdersService.GetOrdersByOrderNoAndRestaurantId(OrderNo, RestaurantId);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetOrdersByOrderNoAndRestaurantId");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Get Info order 
        [HttpGet()]
        public async Task<IActionResult> GetAll(string? OrderNo, string? RestaurantName, DateTime datefrom, DateTime dateto)
        {
            try
            {
                ResObj res = await _OrdersService.GetAll(OrderNo, RestaurantName, datefrom, dateto);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetAll => name:");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region delete Info Orders 
        [HttpDelete("Delete/{OrderId}")]
        public async Task<IActionResult> Delete(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.Delete(OrderId);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => Delete");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region delete Info Orders Details
        [HttpDelete("DeleteDetails/{OrderDetailId}")]
        public async Task<IActionResult> DeleteDetails(int OrderDetailId)
        {
            try
            {
                ResObj res = await _OrdersService.DeleteDetails(OrderDetailId);

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => DeleteDetails");
                return Response(false, "حدث خطا اثناء عملية الحذف");
            }
        }
        #endregion

        #region Get Orders ById Info Orders 
        [HttpGet("GetById/{OrderId}")]
        public async Task<IActionResult> GetById(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.GetById(OrderId);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetById");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion


        #region insert  Info orders      
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrdersModel  ordersModel)
        {
            try
            {

                Orders Orders = _mapper.Map<Orders>(ordersModel);
                ResObj resuser = await _OrdersService.PostUser(ordersModel.Users);
                  
                if (resuser.success)
                {
                    Users user = (Users)resuser.data;
                   Orders.UserId=user.UserId;
                   Orders.UserName=user.Name;
                }

                ResObj res;
                res = await _OrdersService.Post(Orders);
                Orders orders=(Orders)res.data;
                List<string> ids = new List<string>();   
              
                ids.Add(orders.UserId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلبك",
                    Details =  " سوف يتم الموافقة على طلبك قريبا  ",
                    DateInsert = Key.DateTimeIQ,
                    UserId = orders.UserId,ResId=0   ,Images=""
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderUser(notifications.Title, notifications.Details,
                      ids);
                }
                catch (Exception ex) { }  
                List<string> ids1 = new List<string>();
                
                ids1.Add(orders.RestaurantId.ToString());
                Notification notifications1 = new Notification
                {
                    Title = "طلب",
                    Details = " بطلب منتجات بانتظار الموافقة " + orders.UserName + "قام ",
                    DateInsert = Key.DateTimeIQ,
                    ResId = orders.RestaurantId, UserId=0,Images=""
                };                                     
                await _noteService.Post(notifications1);
                try
                {
                    await OneSignalSenderUser(notifications1.Title, notifications1.Details,
                      ids1);
                }
                catch (Exception ex) { }
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => Post");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Set Order IsApporve
        [HttpPost("SetIsApporve/{OrderId}")]
        public async Task<IActionResult> SetIsApporve(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetIsApporve(OrderId);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Orders orders = (Orders)res.data;
                List<string> ids = new List<string>();
                string Name = "";
               // Name = await _Userservice.GetNamePersonById(orders.UserId);

                ids.Add(orders.UserId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلبك",
                    Details = $" يرجى انتظار لتجهيز طلبك {Name} تم تجهيز طلبك من متجر",
                    DateInsert = Key.DateTimeIQ,
                    UserId = orders.UserId
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderUser(notifications.Title, notifications.Details,
                      ids);
                }
                catch (Exception ex) { }
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetIsApporve");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion 

        #region Set Order IsCancel
        [HttpDelete("SetIsCancel/{OrderId}")]
        public async Task<IActionResult> SetIsCancel(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetIsCancel(OrderId);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Orders orders = (Orders)res.data;
                List<string> ids = new List<string>();
                string Name = "";
                //Name = await _Userservice.GetNamePersonById(orders.UserId);

                ids.Add(orders.UserId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلبك",
                    Details = $"  {Name} تم الغاء طلبك من متجر",
                    DateInsert = Key.DateTimeIQ,
                    UserId = orders.UserId ,      
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderUser(notifications.Title, notifications.Details,
                      ids);
                }
                catch (Exception ex) { }

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetIsCancel");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
                              
        #region Set Order IsDone
        [HttpPost("SetIsDone/{OrderId}")]
        public async Task<IActionResult> SetIsDone(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetIsDone(OrderId);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Orders orders = (Orders)res.data;
                List<string> ids = new List<string>();
                string Name = "";
              //  UserName = await _Userservice.GetNamePersonById(orders.UserId);

                ids.Add(orders.UserId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلبك",
                    Details = $" يرجى انتظار توصيل الطلب الى بيتكتم {Name} تم تجهيز طلبك من متجر",
                    DateInsert = Key.DateTimeIQ,
                    UserId = orders.UserId
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderUser(notifications.Title, notifications.Details,
                      ids);
                }
                catch (Exception ex) { }
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetIsDone");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion


    }
}
