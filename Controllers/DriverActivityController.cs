using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;

namespace RomanaWeb.Controllers
{
    [Authorize]
    [Route("driver-activity")]
    public class DriverActivityController : MasterController
    {
        private readonly DB_Context _context;
        private readonly ILoggerRepository _logger;

        public DriverActivityController(DB_Context context, ILoggerRepository logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool IsAdmin() =>
            UserManager != null && string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        [HttpGet]
        public async Task<IActionResult> Report(DateTime? dateFrom, DateTime? dateTo, string? name)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var rows = await BuildRowsAsync(dateFrom, dateTo, name);
                return Response(true, rows);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverActivityController => Report");
                return Response(false, "خطأ");
            }
        }

        [HttpGet("{saleManId:int}/orders")]
        public async Task<IActionResult> DriverOrders(int saleManId, DateTime? dateFrom, DateTime? dateTo)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");

                var driver = await _context.SaleMan.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SaleManId == saleManId && s.IsDelete != true);
                if (driver == null) return Response(false, "المندوب غير موجود");

                var rangeFrom = (dateFrom ?? DateTime.Today.AddDays(-30)).Date;
                var rangeTo = (dateTo ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

                var orders = await (
                    from o in _context.Orders.AsNoTracking()
                    where o.SaleManId == saleManId && o.OrderDate >= rangeFrom && o.OrderDate <= rangeTo
                    join r in _context.Restaurant.AsNoTracking() on o.RestaurantId equals r.RestaurantId into rj
                    from r in rj.DefaultIfEmpty()
                    join u in _context.Users.AsNoTracking() on o.UserId equals u.UserId into uj
                    from u in uj.DefaultIfEmpty()
                    orderby o.OrderDate descending
                    select new
                    {
                        o.OrderId,
                        o.OrderNo,
                        o.OrderDate,
                        o.RestaurantId,
                        RestaurantName = r != null ? r.Name : null,
                        RestaurantAddress = r != null ? r.Address : null,
                        CustomerName = u != null ? u.Name : null,
                        CustomerPhone = u != null ? u.Phone : null,
                        DeliveryAddress = u != null ? u.Address : null,
                        o.NetAmount,
                        o.Total,
                        o.CostDelivery,
                        o.RouteDistanceKm,
                        o.PricingFromZone,
                        o.PricingToZone,
                        o.IsCancel,
                        o.IsDelivered,
                        o.IsDeliveryConfirmed,
                        o.IsOutForDelivery,
                        o.IsPickedUpFromRestaurant,
                        o.IsDriverEnRouteToPickup,
                        o.IsSaleManApprove,
                        o.IsPreparing,
                        o.IsApporve,
                        o.Notes,
                        o.Lat,
                        o.Long
                    }
                ).ToListAsync();

                var details = orders.Select(o =>
                {
                    var status = ResolveStatus(o.IsCancel, o.IsDeliveryConfirmed, o.IsDelivered,
                        o.IsOutForDelivery, o.IsPickedUpFromRestaurant, o.IsDriverEnRouteToPickup,
                        o.IsSaleManApprove, o.IsPreparing, o.IsApporve);

                    var deliveryAddress = !string.IsNullOrWhiteSpace(o.DeliveryAddress)
                        ? o.DeliveryAddress
                        : (!string.IsNullOrWhiteSpace(o.Lat) && !string.IsNullOrWhiteSpace(o.Long)
                            ? $"إحداثيات: {o.Lat}, {o.Long}"
                            : "—");

                    return new DriverOrderDetailRow
                    {
                        OrderId = o.OrderId,
                        OrderNo = o.OrderNo,
                        OrderDate = o.OrderDate,
                        RestaurantName = o.RestaurantName ?? "—",
                        RestaurantAddress = o.RestaurantAddress,
                        CustomerName = o.CustomerName ?? "—",
                        CustomerPhone = o.CustomerPhone,
                        DeliveryAddress = deliveryAddress,
                        OrderAmount = (decimal)(o.NetAmount > 0 ? o.NetAmount : o.Total),
                        DeliveryFee = o.CostDelivery ?? 0m,
                        RouteDistanceKm = o.RouteDistanceKm,
                        FromZone = o.PricingFromZone,
                        ToZone = o.PricingToZone,
                        StatusCode = status.Code,
                        StatusText = status.Text,
                        StatusGroup = status.Group,
                        Notes = o.Notes
                    };
                }).ToList();

                return Response(true, new
                {
                    driver = new
                    {
                        saleManId = driver.SaleManId,
                        name = driver.Name,
                        phone = driver.Phone,
                        isAvailable = driver.IsAvailable
                    },
                    dateFrom = rangeFrom,
                    dateTo = rangeTo.Date,
                    summary = new
                    {
                        totalOrders = details.Count,
                        delivered = details.Count(d => d.StatusGroup == "delivered"),
                        cancelled = details.Count(d => d.StatusGroup == "cancelled"),
                        active = details.Count(d => d.StatusGroup == "active"),
                        totalDeliveryFees = details.Sum(d => d.DeliveryFee),
                        totalRouteKm = details.Sum(d => d.RouteDistanceKm ?? 0m)
                    },
                    orders = details
                });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverActivityController => DriverOrders");
                return Response(false, "خطأ");
            }
        }

        [HttpGet("excel")]
        public async Task<IActionResult> Excel(DateTime? dateFrom, DateTime? dateTo, string? name)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var rows = await BuildRowsAsync(dateFrom, dateTo, name);

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("نشاط المندوبين");
                ws.Cell(1, 1).Value = "اسم المندوب";
                ws.Cell(1, 2).Value = "الهاتف";
                ws.Cell(1, 3).Value = "حالة العمل";
                ws.Cell(1, 4).Value = "طلبات معينة";
                ws.Cell(1, 5).Value = "تم التوصيل";
                ws.Cell(1, 6).Value = "ملغي";
                ws.Cell(1, 7).Value = "رسوم توصيل (د.ع)";
                ws.Cell(1, 8).Value = "مسافة (كم)";
                ws.Range(1, 1, 1, 8).Style.Font.Bold = true;

                int r = 2;
                foreach (var row in rows)
                {
                    ws.Cell(r, 1).Value = row.DriverName;
                    ws.Cell(r, 2).Value = row.Phone;
                    ws.Cell(r, 3).Value = row.IsAvailable ? "يعمل" : "متوقف";
                    ws.Cell(r, 4).Value = row.AssignedOrders;
                    ws.Cell(r, 5).Value = row.DeliveredOrders;
                    ws.Cell(r, 6).Value = row.CancelledOrders;
                    ws.Cell(r, 7).Value = row.TotalDeliveryFees;
                    ws.Cell(r, 8).Value = row.TotalRouteKm;
                    r++;
                }
                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                string fileName = "driver-activity-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".xlsx";
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "DriverActivityController => Excel");
                return Response(false, "خطأ");
            }
        }

        private async Task<List<DriverActivityRow>> BuildRowsAsync(DateTime? dateFrom, DateTime? dateTo, string? name)
        {
            var from = (dateFrom ?? DateTime.Today.AddDays(-30)).Date;
            var to = (dateTo ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

            var driversQuery = _context.SaleMan.AsNoTracking()
                .Where(s => s.IsDelete != true);
            if (!string.IsNullOrWhiteSpace(name))
                driversQuery = driversQuery.Where(s => s.Name.Contains(name.Trim()));

            var drivers = await driversQuery.OrderBy(s => s.Name).ToListAsync();

            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.SaleManId > 0 && o.OrderDate >= from && o.OrderDate <= to)
                .Select(o => new
                {
                    o.SaleManId,
                    o.IsCancel,
                    o.IsDelivered,
                    o.IsDeliveryConfirmed,
                    o.CostDelivery,
                    o.RouteDistanceKm
                })
                .ToListAsync();

            var stats = orders
                .GroupBy(o => o.SaleManId!.Value)
                .ToDictionary(g => g.Key, g => new
                {
                    Assigned = g.Count(),
                    Delivered = g.Count(x => x.IsDelivered == true || x.IsDeliveryConfirmed),
                    Cancelled = g.Count(x => x.IsCancel),
                    Fees = g.Sum(x => x.CostDelivery ?? 0m),
                    Km = g.Sum(x => x.RouteDistanceKm ?? 0m)
                });

            return drivers.Select(d =>
            {
                stats.TryGetValue(d.SaleManId, out var s);
                return new DriverActivityRow
                {
                    SaleManId = d.SaleManId,
                    DriverName = d.Name,
                    Phone = d.Phone,
                    IsAvailable = d.IsAvailable,
                    IsActive = d.IsActive != false,
                    AssignedOrders = s?.Assigned ?? 0,
                    DeliveredOrders = s?.Delivered ?? 0,
                    CancelledOrders = s?.Cancelled ?? 0,
                    TotalDeliveryFees = s?.Fees ?? 0m,
                    TotalRouteKm = s?.Km ?? 0m
                };
            }).ToList();
        }

        private static (int Code, string Text, string Group) ResolveStatus(
            bool isCancel, bool isDeliveryConfirmed, bool? isDelivered,
            bool isOutForDelivery, bool isPickedUp, bool isEnRoute,
            bool? isSaleManApprove, bool isPreparing, bool isApprove)
        {
            if (isCancel) return (9, "ملغي", "cancelled");
            if (isDeliveryConfirmed) return (8, "تم التأكيد", "delivered");
            if (isDelivered == true) return (7, "تم التوصيل", "delivered");
            if (isOutForDelivery) return (6, "في الطريق للزبون", "active");
            if (isPickedUp) return (5, "تم الاستلام من المطعم", "active");
            if (isEnRoute) return (4, "في الطريق للمطعم", "active");
            if (isSaleManApprove == true) return (3, "قبل المندوب", "active");
            if (isPreparing) return (2, "قيد التحضير", "active");
            if (isApprove) return (1, "موافق عليه", "active");
            return (0, "قيد الانتظار", "active");
        }

        public class DriverActivityRow
        {
            public int SaleManId { get; set; }
            public string DriverName { get; set; } = "";
            public string Phone { get; set; } = "";
            public bool IsAvailable { get; set; }
            public bool IsActive { get; set; }
            public int AssignedOrders { get; set; }
            public int DeliveredOrders { get; set; }
            public int CancelledOrders { get; set; }
            public decimal TotalDeliveryFees { get; set; }
            public decimal TotalRouteKm { get; set; }
        }

        public class DriverOrderDetailRow
        {
            public int OrderId { get; set; }
            public int OrderNo { get; set; }
            public DateTime OrderDate { get; set; }
            public string RestaurantName { get; set; } = "";
            public string? RestaurantAddress { get; set; }
            public string CustomerName { get; set; } = "";
            public string? CustomerPhone { get; set; }
            public string DeliveryAddress { get; set; } = "";
            public decimal OrderAmount { get; set; }
            public decimal DeliveryFee { get; set; }
            public decimal? RouteDistanceKm { get; set; }
            public string? FromZone { get; set; }
            public string? ToZone { get; set; }
            public int StatusCode { get; set; }
            public string StatusText { get; set; } = "";
            public string StatusGroup { get; set; } = "active";
            public string? Notes { get; set; }
        }
    }
}
