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
   // [Authorize]
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
        [HttpGet("GetOrdersByOrderNoAndUserId/{OrderNo},{PersonId}")]
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
        [HttpGet("GetOrdersByOrderNoAndRestaurantId/{OrderNo},{RestaurantId},{Type}")]
        public async Task<IActionResult> GetOrdersByOrderNoAndRestaurantId(string? OrderNo, int? RestaurantId,int Type)
        {
            try
            {
                ResObj res = await _OrdersService.GetOrdersByOrderNoAndRestaurantId(OrderNo, RestaurantId, Type);       
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetOrdersByOrderNoAndRestaurantId");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion   
        #region Get Orders By OrderNo and RestaurantId 
        [HttpGet("GetOrdersByOrderNoAndSaleManId/{OrderNo},{SaleManId},{Type}")]
        public async Task<IActionResult> GetOrdersByOrderNoAndSaleManId(string? OrderNo, int? SaleManId, int Type)
        {
            try
            {
                ResObj res = await _OrdersService.GetOrdersByOrderNoAndSaleManId(OrderNo, SaleManId, Type);       
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetOrdersByOrderNoAndSaleManId");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Get Info order 
        [HttpGet()]
        public async Task<IActionResult> GetAll(string? OrderNo, string? RestaurantName, DateTime datefrom, DateTime dateto,int? CountriesId,int? state)
        {
            try
            {
                int? RestaurantId = 0;
                if (UserManager.Role == "res")
                {
                    RestaurantId = UserManager.Id;
                }
                ResObj res = await _OrdersService.GetAll(OrderNo, RestaurantName, datefrom, dateto,RestaurantId, CountriesId, state);

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
                if (res.success == false)
                {         
                    return Response(res.success, res.msg, res.data);
                }
                Orders orders=(Orders)res.data;
                List<string> ids = new List<string>();   
              
                ids.Add(orders.UserId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلبك",
                    Details = $" مرحبا {orders.UserName} لقد استلمنا طلبك بنجاح ، فضلاً انتظر الموافقة عليه  ",
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
                    Details =  $" قام {orders.UserName} بطلب منتجات بانتظار الموافقة ",
                    DateInsert = Key.DateTimeIQ,
                    ResId = orders.RestaurantId, UserId=0,Images=""
                };                                     
                await _noteService.Post(notifications1);
                try
                {
                    await OneSignalSenderRes(notifications1.Title, notifications1.Details,
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
        [HttpPost("Orders/SetIsApporve/{OrderId}")]
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
                Name = await _OrdersService.GetNamePersonById(orders.UserId);

                ids.Add(orders.UserId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلبك",
                    Details = $" مرحبا {Name}  تمت الموافقة على الطلب ، يتم تجهيز طلبك ",
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
        [HttpDelete("Orders/SetIsCancel/{OrderId}")]
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
                Name = await _OrdersService.GetNamePersonById(orders.UserId);

                ids.Add(orders.UserId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلبك",
                    Details = $"مرحبا {Name} نعتذر عن عدم قبول طلبك . يمكنك تجربة الطلب مرة اخرى . ",
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

        #region Set Order SetSaleManId
        [HttpPost("Orders/SetSaleManId/{OrderId},{SaleManId}")]
        public async Task<IActionResult> SetSaleManId(int OrderId,int SaleManId)    
        {
            try
            {
                ResObj res = await _OrdersService.SetSaleManId(OrderId,SaleManId);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Orders orders = (Orders)res.data;
                // for saleman
                List<string> ids = new List<string>();
                string Name = "";
                Name = await _OrdersService.GetSaleManPersonById(SaleManId);

                ids.Add(orders.SaleManId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلب جديد",
                    Details = $"هلو {Name} نود تبليغك  ، ان هناك طلب قادم اليك .",
                    DateInsert = Key.DateTimeIQ,     
                    SaleManId = SaleManId,ResId=0,UserId=0
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderSal(notifications.Title, notifications.Details,
                      ids);
                }
                catch (Exception ex) { }

              
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetSaleManId");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion       

        #region Set Order IsDone
        [HttpPost("Orders/SetIsDone/{OrderId}")]
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
                Name = await _OrdersService.GetNamePersonById(orders.UserId);

                ids.Add(orders.UserId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلبك",
                    Details = $"مرحبا {Name}سلمنا طلبك الى شركة التوصيل انتظر تواصلهم معك.",
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

        #region Set Order SetIsSaleManApprove
        [HttpPost("Orders/SetIsSaleManApprove/{OrderId}")]
        public async Task<IActionResult> SetIsSaleManApprove(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetIsSaleManApprove(OrderId);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Orders orders = (Orders)res.data;
                List<string> ids = new List<string>();                       

                ids.Add(orders.RestaurantId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلب المندوب",
                    Details = $" {orders.OrderNo} وافق مندوبك على طلب رقم ",
                    DateInsert = Key.DateTimeIQ,
                    ResId = orders.RestaurantId   ,UserId=0,SaleManId=0
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderRes(notifications.Title, notifications.Details,
                      ids);
                }
                catch (Exception ex) { }
                // for user
                List<string> idsuser = new List<string>();
                string Nameuser = "";
                Nameuser = await _OrdersService.GetNamePersonById(orders.UserId);

                idsuser.Add(orders.UserId.ToString());
                Notification notificationsuser = new Notification
                {
                    Title = "طلبك",
                    Details = $"هلو {Nameuser} نود تبليغك  ، ان هناك طلبك بيد المندوب .",
                    DateInsert = Key.DateTimeIQ,
                    SaleManId = 0,
                    ResId = 0,
                    UserId = orders.UserId
                };
                await _noteService.Post(notificationsuser);
                try
                {
                    await OneSignalSenderUser(notificationsuser.Title, notificationsuser.Details,
                      ids);
                }
                catch (Exception ex) { }

                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetIsSaleManApprove");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Set Order SetIsSaleManCancel
        [HttpPost("Orders/SetIsSaleManCancel/{OrderId}")]
        public async Task<IActionResult> SetIsSaleManCancel(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetIsSaleManCancel(OrderId);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Orders orders = (Orders)res.data;   
                List<string> ids = new List<string>();                           

                ids.Add(orders.RestaurantId.ToString());
                Notification notifications = new Notification
                {
                    Title = "طلب المندوب",
                    Details = $" {orders.OrderNo} تم الغاء الطلب من قبل مندوبك على طلب رقم ",
                    DateInsert = Key.DateTimeIQ,
                    ResId = orders.RestaurantId   ,UserId=0 , SaleManId=0,Images=""
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderRes(notifications.Title, notifications.Details,
                      ids);
                }
                catch (Exception ex) { }
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetIsSaleManCancel");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion


    }
}
