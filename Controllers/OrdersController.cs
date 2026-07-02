using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using RomanaWeb.Helper.Interface;
using AutoMapper;
using RomanaWeb.Models.EntityMapper;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Repository;
using System.Data;
using ClosedXML.Excel;
namespace RomanaWeb.Controllers
{
    // [Authorize]
    public class OrdersController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IOrdersService _OrdersService;
        private readonly INotificationService _noteService;
        private readonly IOrderHubNotifier _hubNotifier;
        public readonly IMapper _mapper;
        #endregion

        #region Const
        public OrdersController(
            ILoggerRepository logger,
            IOrdersService OrdersService,
            INotificationService noteService,
            IOrderHubNotifier hubNotifier,
            IMapper mapper)
        {
            _logger = logger;
            _OrdersService = OrdersService;
            _noteService = noteService;
            _hubNotifier = hubNotifier;
            _mapper = mapper;
        }

        private static string ResolveDisplayMode(string statusKey) =>
            statusKey == "new_order" ? "fullscreen" : "banner";

        private async Task PushOrderSignalR(Orders order, string title, string msg, string statusKey,
            bool notifyUser = false, bool notifyRestaurant = false, bool notifyDriver = false, bool notifyAllDrivers = false)
        {
            try
            {
                int statusCode = OrdersService.MapOrderStatus(order);
                string displayMode = ResolveDisplayMode(statusKey);
                if (notifyUser && order.UserId > 0)
                    await _hubNotifier.NotifyUserAsync(order.UserId, title, msg, order.OrderId, statusKey, statusCode, displayMode);
                if (notifyRestaurant && order.RestaurantId > 0)
                    await _hubNotifier.NotifyRestaurantAsync(order.RestaurantId, title, msg, order.OrderId, statusKey, statusCode, displayMode);
                if (notifyDriver && order.SaleManId is > 0)
                    await _hubNotifier.NotifyDriverAsync(order.SaleManId.Value, title, msg, order.OrderId, statusKey, statusCode, displayMode);
                if (notifyAllDrivers)
                    await _hubNotifier.NotifyAllDriversAsync(title, msg, order.OrderId, statusKey, statusCode, displayMode);
            }
            catch { }
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
        public async Task<IActionResult> GetAll(string? OrderNo, string? RestaurantName, DateTime datefrom, DateTime dateto,int? CountriesId,int? state,
            string? Phone = null, int? orderStatus = null)
        {
            try
            {
                int? RestaurantId = 0;
                if (UserManager?.Role == "res")
                {
                    RestaurantId = UserManager.Id;
                }
                ResObj res = await _OrdersService.GetAll(OrderNo, RestaurantName, datefrom, dateto,RestaurantId, CountriesId, state, Phone, orderStatus);

                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetAll => name:");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Get Info Delivery 
        [AllowAnonymous]
        [HttpGet]
        public async Task<FileResult> GetExcelAll(string? OrderNo, string? RestaurantName, DateTime datefrom, DateTime dateto, int? CountriesId)
        {
            try
            {
                ResObj res = await _OrdersService.GetAll(OrderNo, RestaurantName, datefrom, dateto, 0, CountriesId, 1);


                var p = GenerateExcel("report-order-" + Key.DateTimeIQ + ".xlsx", (List<Orders>)res.data);
                return p;
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetExcelAll");
                return null;
            }
        }
        [NonAction]
        private FileResult GenerateExcel(string fileName, IEnumerable<Orders> people)
        {
            DataTable dataTable = new DataTable("order");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("OrderId"),
                new DataColumn("OrderNo"),
                new DataColumn("RestaurantName"),
                new DataColumn("ResPhone"),
                new DataColumn("UserName"),
                new DataColumn("Address"),
                new DataColumn("Phone"),
                new DataColumn("FunctionPoint"),
                new DataColumn("NetAmount"),
                new DataColumn("CountriesName"),
                new DataColumn("CityName"),
            });

            foreach (var p in people)
            {
                dataTable.Rows.Add(p.OrderId, p.OrderNo, p.RestaurantName,p.ResPhone, p.UserName, p.Address, p.Phone,
                   p.FunctionPoint,
                    p.NetAmount, p.CountriesName, p.CityName
                    );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);

                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
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
        [HttpPost("Orders/Post")]
        public async Task<IActionResult> Post([FromBody] OrdersModel  ordersModel)
        {
            try
            {                                                                   
                Orders Orders = _mapper.Map<Orders>(ordersModel);

                if (Orders.NetAmount == 0)
                {
                    return Response(false, "مبلغ الفاتورة 0");
                }

                var u = ordersModel.Users;
                if (u == null ||
                    !double.TryParse(u.Lat, out var lat) || lat == 0 ||
                    !double.TryParse(u.Long, out var lng) || lng == 0 ||
                    u.CityId == null || u.CityId == 0)
                {
                    return Response(false, "يجب تحديد الموقع والمنطقة");
                }

                ResObj resuser = await _OrdersService.PostUser(ordersModel.Users);
                                                                               
                if (resuser.success)
                {
                    Users user = (Users)resuser.data;
                   Orders.UserId=user.UserId;
                   Orders.UserName=user.Name;
                   Orders.Lat = user.Lat;
                   Orders.Long = user.Long;
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
                    Title = "تم استلام طلبك",
                    Details = "طلبك وصل للمطعم وبانتظار التاكيد.",
                    DateInsert = Key.DateTimeIQ,
                    UserId = orders.UserId,ResId=0   ,Images=""
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderUserWithOrder(notifications.Title, notifications.Details, ids,
                        orderId: orders.OrderId, statusKey: "pending", statusAr: "انتظار", accentArgb: "FFEF6C00");
                }
                catch (Exception ex) { }  
                List<string> ids1 = new List<string>();
                
                ids1.Add(orders.RestaurantId.ToString());
                Notification notifications1 = new Notification
                {
                    Title = "طلب جديد وارد",
                    Details = $"لديك طلب جديد من {orders.UserName} بانتظار المراجعة.",
                    DateInsert = Key.DateTimeIQ,
                    ResId = orders.RestaurantId, UserId=0,Images=""
                };                                     
                await _noteService.Post(notifications1);
                try
                {
                    await OneSignalSenderResWithOrder(notifications1.Title, notifications1.Details, ids1,
                        orders.OrderId, "new_order", "طلب جديد", "FFEF6C00", statusCode: 0, displayMode: "fullscreen");
                }
                catch (Exception ex) { }
                await PushOrderSignalR(orders, notifications1.Title, notifications1.Details, "new_order",
                    notifyRestaurant: true, notifyAllDrivers: true);
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
                    Title = "تم تأكيد الطلب",
                    Details = "بدا المطعم بتحضير طلبك.",
                    DateInsert = Key.DateTimeIQ,
                    UserId = orders.UserId
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderUserWithOrder(notifications.Title, notifications.Details, ids,
                        orderId: orders.OrderId, statusKey: "approved", statusAr: "مقبول", accentArgb: "FF1B5E20");
                }
                catch (Exception ex) { }
                await PushOrderSignalR(orders, notifications.Title, notifications.Details, "approved", notifyUser: true);
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
                    Title = "تم رفض الطلب",
                    Details = "نعتذر، تم رفض الطلب. يمكنك المحاولة مرة اخرى.",
                    DateInsert = Key.DateTimeIQ,
                    UserId = orders.UserId ,      
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderUserWithOrder(notifications.Title, notifications.Details, ids,
                        orderId: orders.OrderId, statusKey: "cancel", statusAr: "مرفوض", accentArgb: "FFD32F2F");
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

        #region Set Order SetSaleManId (DEPRECATED - Section 4.2)
        // Section 4.2: manual "assign driver" was removed from the order screen.
        // Drivers self-accept via DriverDispatchController. This endpoint stays for
        // legacy callers but returns 410 unless feature flag Drivers:AllowManualAssign is on.
        [HttpPost("Orders/SetSaleManId/{OrderId},{SaleManId}")]
        public async Task<IActionResult> SetSaleManId(int OrderId,int SaleManId,
            [FromServices] Microsoft.Extensions.Configuration.IConfiguration config)
        {
            try
            {
                if (!config.GetValue<bool>("Drivers:AllowManualAssign", false))
                    return StatusCode(410, new { success = false, msg = "Manual driver assignment is disabled. Drivers self-accept (Section 4.2)." });

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
                    Title = "طلب جديد متاح",
                    Details = $"يوجد طلب قريب منك بانتظار القبول يا {Name}.",
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
                    Title = "تم تعيين سائق",
                    Details = "تم العثور على سائق قريب لتوصيل طلبك.",
                    DateInsert = Key.DateTimeIQ,
                    UserId = orders.UserId
                };
                await _noteService.Post(notifications);
                try
                {
                    await OneSignalSenderUserWithOrder(notifications.Title, notifications.Details, ids,
                        orderId: orders.OrderId, statusKey: "done", statusAr: "منتهي", accentArgb: "FF2E7D32");
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
                    Title = "تم قبول الطلب",
                    Details = $"تم قبول طلب رقم {orders.OrderNo} من قبل السائق.",
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
                    Title = "تم تعيين سائق",
                    Details = $"تم تعيين سائق لطلبك رقم {orders.OrderNo}.",
                    DateInsert = Key.DateTimeIQ,
                    SaleManId = 0,
                    ResId = 0,
                    UserId = orders.UserId
                };
                await _noteService.Post(notificationsuser);
                try
                {
                    await OneSignalSenderUserWithOrder(notificationsuser.Title, notificationsuser.Details, idsuser,
                        orderId: orders.OrderId, statusKey: "approved", statusAr: "مقبول", accentArgb: "FF1B5E20");
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
                    Title = "تم إلغاء الطلب",
                    Details = $"تم إلغاء قبول السائق لطلب رقم {orders.OrderNo}.",
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

        #region Set Order SetIsDelivered
        [HttpPost("Orders/SetIsDelivered/{OrderId}")]
        public async Task<IActionResult> SetIsDelivered(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetIsDelivered(OrderId);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Orders orders = (Orders)res.data;
                List<string> ids = new List<string>();

                ids.Add(orders.RestaurantId.ToString());
                Notification notifications = new Notification
                {
                    Title = "تم تسليم الطلب",
                    Details = $"تم تسليم الطلب رقم {orders.OrderNo} بنجاح.",
                    DateInsert = Key.DateTimeIQ,
                    ResId = orders.RestaurantId,
                    UserId = 0,
                    SaleManId = 0
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
                await _logger.WriteAsync(ex, "OrdersController => SetIsDelivered");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Set Order SetIsNotDelivered
        [HttpPost("Orders/SetIsNotDelivered/{OrderId}/{Reason}")]
        public async Task<IActionResult> SetIsNotDelivered(int OrderId,string Reason)
        {
            try
            {
                ResObj res = await _OrdersService.SetIsNotDelivered(OrderId, Reason);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Orders orders = (Orders)res.data;
                List<string> ids = new List<string>();

                ids.Add(orders.RestaurantId.ToString());
                Notification notifications = new Notification
                {
                    Title = "تعذر تسليم الطلب",
                    Details = $"تم رفض استلام الطلب رقم {orders.OrderNo}. السبب: {orders.Reason}",
                    DateInsert = Key.DateTimeIQ,
                    ResId = orders.RestaurantId,
                    UserId = 0,
                    SaleManId = 0
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
                await _logger.WriteAsync(ex, "OrdersController => SetIsNotDelivered");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion      
        #region Set Order SetIsWaiting
        [HttpPost("Orders/SetIsWaiting/{OrderId}/{Reason2}")]
        public async Task<IActionResult> SetIsWaiting(int OrderId,string Reason2)
        {
            try
            {
                ResObj res = await _OrdersService.SetIsWaiting(OrderId, Reason2);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Orders orders = (Orders)res.data;
                List<string> ids = new List<string>();

                ids.Add(orders.RestaurantId.ToString());
                Notification notifications = new Notification
                {
                    Title = "تأجيل الطلب",
                    Details = $"تم تأجيل الطلب رقم {orders.OrderNo}. السبب: {orders.Reason2}",
                    DateInsert = Key.DateTimeIQ,
                    ResId = orders.RestaurantId,
                    UserId = 0,
                    SaleManId = 0
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
                await _logger.WriteAsync(ex, "OrdersController => SetIsWaiting");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Modify Order - Delete old details and add new ones
        [HttpPost]
        public async Task<IActionResult> ModifyOrder(int OrderId, [FromBody] List<OrderDetail> newDetails)
        {
            try
            {
                ResObj res = await _OrdersService.ModifyOrder(OrderId, newDetails);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => ModifyOrder");
                return Response(false, "حدث خطا اثناء عملية تعديل الطلب");
            }
        }
        #endregion

        #region Add single product to existing order
        [HttpPost]
        public async Task<IActionResult> AddOrderDetail([FromBody] OrderDetail detail)
        {
            try
            {
                ResObj res = await _OrdersService.AddOrderDetail(detail);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => AddOrderDetail");
                return Response(false, "حدث خطا اثناء عملية اضافة المنتج");
            }
        }
        #endregion

        #region Get Nearby Driver Orders (for mobile app)
        [Authorize]
        [HttpGet("Orders/GetNearbyDriverOrders/{Lat},{Lng},{RadiusKm}")]
        public async Task<IActionResult> GetNearbyDriverOrders( double Lat, double Lng, double RadiusKm)
        {
            try
            {
                if(UserManager.Role== "sal")
                {
                    ResObj res = await _OrdersService.GetNearbyDriverOrders(UserManager.Id, Lat, Lng, RadiusKm);
                    return Response(res.success, res.data);
                }
                return Response(false, "Unauthorized");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetNearbyDriverOrders");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Approve Order By SaleMan (driver claims order)
        [HttpPost("Orders/ApproveOrderBySaleMan/{OrderId},{SaleManId}")]
        public async Task<IActionResult> ApproveOrderBySaleMan(int OrderId, int SaleManId)
        {
            try
            {
                ResObj res = await _OrdersService.ApproveOrderBySaleMan(OrderId, SaleManId);
                if (!res.success)
                    return Response(res.success, res.msg);

                Orders orders = (Orders)res.data;

                // Notify the restaurant
                List<string> ids = new List<string>();
                ids.Add(orders.RestaurantId.ToString());
                string driverName = await _OrdersService.GetSaleManPersonById(SaleManId);
                Notification notification = new Notification
                {
                    Title = "تم قبول الطلب",
                    Details = $"المندوب {driverName} قبل توصيل الطلب رقم {orders.OrderNo}.",
                    DateInsert = Key.DateTimeIQ,
                    ResId = orders.RestaurantId,
                    UserId = 0,
                    SaleManId = 0,
                    Images = ""
                };
                await _noteService.Post(notification);
                try { await OneSignalSenderRes(notification.Title, notification.Details, ids); }
                catch { }

                // Notify the user
                List<string> userIds = new List<string>();
                userIds.Add(orders.UserId.ToString());
                string userName = await _OrdersService.GetNamePersonById(orders.UserId);
                Notification userNotif = new Notification
                {
                    Title = "تم تعيين سائق",
                    Details = $"تم تعيين السائق {driverName} لتوصيل طلبك رقم {orders.OrderNo}.",
                    DateInsert = Key.DateTimeIQ,
                    UserId = orders.UserId,
                    ResId = 0,
                    SaleManId = 0,
                    Images = ""
                };
                await _noteService.Post(userNotif);
                try
                {
                    await OneSignalSenderUserWithOrder(userNotif.Title, userNotif.Details, userIds,
                        orders.OrderId, "driver_assigned", "تم تعيين سائق", "FF1565C0", statusCode: 3, displayMode: "banner");
                }
                catch { }
                await PushOrderSignalR(orders, userNotif.Title, userNotif.Details, "driver_assigned", notifyUser: true);

                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => ApproveOrderBySaleMan");
                return Response(false, "حدث خطا اثناء عملية قبول الطلب");
            }
        }
        #endregion

        #region Driver delivery workflow
        [HttpPost("Orders/SetDriverEnRouteToPickup/{OrderId}")]
        public async Task<IActionResult> SetDriverEnRouteToPickup(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetDriverEnRouteToPickup(OrderId);
                if (!res.success) return Response(res.success, res.msg);
                Orders orders = (Orders)res.data;
                string userName = await _OrdersService.GetNamePersonById(orders.UserId);
                var userNotif = new Notification { Title = "السائق متجه إلى المطعم", Details = "السائق في الطريق لاستلام طلبك.", DateInsert = Key.DateTimeIQ, UserId = orders.UserId };
                await _noteService.Post(userNotif);
                var resNotif = new Notification { Title = "السائق متجه إلى المطعم", Details = $"السائق في الطريق لاستلام الطلب رقم {orders.OrderNo}.", DateInsert = Key.DateTimeIQ, ResId = orders.RestaurantId };
                await _noteService.Post(resNotif);
                try
                {
                    await OneSignalSenderUserWithOrder(userNotif.Title, userNotif.Details, new List<string> { orders.UserId.ToString() },
                        orders.OrderId, "driver_en_route", "في الطريق للاستلام", "FF1565C0", statusCode: 4, displayMode: "banner");
                }
                catch { }
                try
                {
                    await OneSignalSenderResWithOrder(resNotif.Title, resNotif.Details, new List<string> { orders.RestaurantId.ToString() },
                        orders.OrderId, "driver_en_route", "السائق متجه", "FF1565C0", statusCode: 4, displayMode: "banner");
                }
                catch { }
                await PushOrderSignalR(orders, userNotif.Title, userNotif.Details, "driver_en_route", notifyUser: true, notifyRestaurant: true);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetDriverEnRouteToPickup");
                return Response(false, "حدث خطأ");
            }
        }

        [HttpPost("Orders/SetPickedUpFromRestaurant/{OrderId}")]
        public async Task<IActionResult> SetPickedUpFromRestaurant(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetPickedUpFromRestaurant(OrderId);
                if (!res.success) return Response(res.success, res.msg);
                Orders orders = (Orders)res.data;
                string userName = await _OrdersService.GetNamePersonById(orders.UserId);
                var notif = new Notification { Title = "تم استلام الطلب من المطعم", Details = "السائق استلم طلبك من المطعم.", DateInsert = Key.DateTimeIQ, UserId = orders.UserId };
                await _noteService.Post(notif);
                try
                {
                    await OneSignalSenderUserWithOrder(notif.Title, notif.Details, new List<string> { orders.UserId.ToString() },
                        orders.OrderId, "picked_up", "تم الاستلام من المطعم", "FF6A1B9A", statusCode: 5, displayMode: "banner");
                }
                catch { }
                await PushOrderSignalR(orders, notif.Title, notif.Details, "picked_up", notifyUser: true, notifyRestaurant: true);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetPickedUpFromRestaurant");
                return Response(false, "حدث خطأ");
            }
        }

        [HttpPost("Orders/SetOutForDelivery/{OrderId}")]
        public async Task<IActionResult> SetOutForDelivery(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetOutForDelivery(OrderId);
                if (!res.success) return Response(res.success, res.msg);
                Orders orders = (Orders)res.data;
                string userName = await _OrdersService.GetNamePersonById(orders.UserId);
                var notif = new Notification { Title = "السائق في الطريق إليك", Details = "طلبك في الطريق للتوصيل.", DateInsert = Key.DateTimeIQ, UserId = orders.UserId };
                await _noteService.Post(notif);
                try
                {
                    await OneSignalSenderUserWithOrder(notif.Title, notif.Details, new List<string> { orders.UserId.ToString() },
                        orders.OrderId, "out_for_delivery", "في الطريق إليك", "FF00838F", statusCode: 6, displayMode: "banner");
                }
                catch { }
                await PushOrderSignalR(orders, notif.Title, notif.Details, "out_for_delivery", notifyUser: true);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetOutForDelivery");
                return Response(false, "حدث خطأ");
            }
        }

        [HttpPost("Orders/SetDeliveryConfirmed/{OrderId}")]
        public async Task<IActionResult> SetDeliveryConfirmed(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.SetDeliveryConfirmed(OrderId);
                if (!res.success) return Response(res.success, res.msg);
                Orders orders = (Orders)res.data;
                string userName = await _OrdersService.GetNamePersonById(orders.UserId);
                var notif = new Notification { Title = "تم تسليم الطلب", Details = "نتمنى لك وجبة شهية", DateInsert = Key.DateTimeIQ, UserId = orders.UserId };
                await _noteService.Post(notif);
                try
                {
                    await OneSignalSenderUserWithOrder(notif.Title, notif.Details, new List<string> { orders.UserId.ToString() },
                        orders.OrderId, "confirmed", "مكتمل", "FF2E7D32", statusCode: 8, displayMode: "banner");
                }
                catch { }
                await PushOrderSignalR(orders, notif.Title, notif.Details, "confirmed", notifyUser: true, notifyRestaurant: true);
                return Response(res.success, res.msg);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => SetDeliveryConfirmed");
                return Response(false, "حدث خطأ");
            }
        }

        [HttpGet("Orders/GetOrderFullDetails/{OrderId}")]
        public async Task<IActionResult> GetOrderFullDetails(int OrderId)
        {
            try
            {
                ResObj res = await _OrdersService.GetOrderFullDetails(OrderId);
                return Response(res.success, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "OrdersController => GetOrderFullDetails");
                return Response(false, "حدث خطأ");
            }
        }
        #endregion

    }
}
