using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;
using System.Data;

namespace RomanaWeb.Controllers
{
    [Authorize]
    public class DeliveryController : MasterController
    {
        #region Readonly 
        private readonly ILoggerRepository _logger;
        private readonly IDeliveryService _DeliveryService;
        private readonly INotificationService _noteService;
        #endregion

        #region Const
        public DeliveryController(
            ILoggerRepository logger,
            IDeliveryService DeliveryService,
            INotificationService noteService)
        {
            _logger = logger;
            _DeliveryService = DeliveryService;
            _noteService = noteService;
        }
        #endregion


        #region Get Orders By No and RestaurantId 
        [HttpGet("GetDeliveryByNoAndRestaurantId/{No},{RestaurantId}")]
        public async Task<IActionResult> GetDeliveryByNoAndRestaurantId(int? No, int? RestaurantId)
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
            , DateTime datefrom, DateTime dateto, int? No=0)
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

        #region Get Info Delivery 
        [AllowAnonymous]
        [HttpGet]
        public async Task<FileResult> GetExcelAll(string? Name,int? CountriesId,string? RestaurantName
            , DateTime datefrom, DateTime dateto, int? No)
        {
            try
            {
                ResObj res = await _DeliveryService.GetAll(Name, CountriesId, RestaurantName, datefrom, dateto, No);


                var p = GenerateExcel("report-delivery-"+Key.DateTimeIQ+".xlsx", (List<Delivery>)res.data);
                return p;
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DeliveryController => GetExcelAll");
                return null;
            }
        }
        [NonAction]
        private FileResult GenerateExcel(string fileName, IEnumerable<Delivery> people)
        {
            DataTable dataTable = new DataTable("Delivery");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("DeliveryId"),
                new DataColumn("No"),
                new DataColumn("RestaurantName"),
                new DataColumn("ResPhone"),
                new DataColumn("UserName"),
                new DataColumn("Address"),
                new DataColumn("Phone"),
                new DataColumn("Notes"),
                new DataColumn("DateInsert"),
                new DataColumn("CostDelivery"),
                new DataColumn("NetAmount"),
                new DataColumn("CountriesName"),
                new DataColumn("CityName"),  
            });

            foreach (var p in people)
            {
                dataTable.Rows.Add(p.DeliveryId,"DE"+p.No, p.RestaurantName,p.ResPhone, 
                    p.Name, p.Address, p.Phone, p.Notes, p.DateInsert,
                    p.CostDelivery, p.NetAmount, p.CountriesName, p.CityName
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


        #region Set Delivery SetIsDelivered
        [AllowAnonymous]
        [HttpPost("Delivery/SetIsDelivered/{OrderId}")]
        public async Task<IActionResult> SetIsDelivered(int OrderId)
        {
            try
            {
                ResObj res = await _DeliveryService.SetIsDelivered(OrderId);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Delivery orders = (Delivery)res.data;
                List<string> ids = new List<string>();

                ids.Add(orders.RestaurantId.ToString());
                Notification notifications = new Notification
                {
                    Title = "توصيل",
                    Details = $" De{orders.No}  عزيزي تم توصيل الطلب الى الزبون بنجاح  رقم الطلب ",
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
                await _logger.WriteAsync(ex, "DeliveryController => SetIsDelivered");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion

        #region Set Delivery SetIsNotDelivered
        [AllowAnonymous]
        [HttpPost("Delivery/SetIsNotDelivered/{OrderId}/{Reason}")]
        public async Task<IActionResult> SetIsNotDelivered(int OrderId, string Reason)
        {
            try
            {
                ResObj res = await _DeliveryService.SetIsNotDelivered(OrderId, Reason);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Delivery orders = (Delivery)res.data;
                List<string> ids = new List<string>();

                ids.Add(orders.RestaurantId.ToString());
                Notification notifications = new Notification
                {
                    Title = "توصيل",
                    Details = $" {orders.Reason} والسبب  De{orders.No}  عزيزي تم رفض الطلب من قبل الزبون رقم الطلب  ",
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
                await _logger.WriteAsync(ex, "DeliveryController => SetIsNotDelivered");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion      
        #region Set Delivery SetIsWaiting
        [AllowAnonymous]
        [HttpPost("Delivery/SetIsWaiting/{OrderId}/{Reason2}")]
        public async Task<IActionResult> SetIsWaiting(int OrderId, string Reason2)
        {
            try
            {
                ResObj res = await _DeliveryService.SetIsWaiting(OrderId, Reason2);
                if (res.success == false)
                {
                    return Response(res.success, res.msg);
                }
                Delivery orders = (Delivery)res.data;
                List<string> ids = new List<string>();

                ids.Add(orders.RestaurantId.ToString());
                Notification notifications = new Notification
                {
                    Title = "توصيل",
                    Details = $" {orders.Reason2} والسبب  De{orders.No}  عزيزي تم تأجيل الطلب  رقم الطلب  ",
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
                await _logger.WriteAsync(ex, "DeliveryController => SetIsWaiting");
                return Response(false, "حدث خطا اثناء عملية جلب البيانات");
            }
        }
        #endregion
    }
}
