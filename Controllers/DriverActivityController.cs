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
    }
}
