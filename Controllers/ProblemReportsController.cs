using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    [Route("problem-reports")]
    public class ProblemReportsController : MasterController
    {
        private readonly DB_Context _context;
        private readonly ILoggerRepository _logger;

        public ProblemReportsController(DB_Context context, ILoggerRepository logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool IsAdmin() =>
            UserManager != null && string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        private bool IsDriver() =>
            UserManager != null && string.Equals(UserManager.Role, "sal", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Driver app: submit a text problem report for an order.
        /// POST /problem-reports
        /// Authorization: Bearer JWT (Role=sal)
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitProblemReportDto? dto)
        {
            try
            {
                if (!IsDriver())
                    return Response(false, "هذه العملية مخصصة لمندوب التوصيل فقط");

                int saleManId = UserManager?.Id ?? 0;
                if (saleManId <= 0)
                    return Response(false, "غير مصرح");

                if (dto == null || dto.OrderId <= 0)
                    return Response(false, "معرف الطلب مطلوب");

                string message = (dto.Message ?? "").Trim();
                if (string.IsNullOrWhiteSpace(message))
                    return Response(false, "نص البلاغ مطلوب");
                if (message.Length > 2000)
                    return Response(false, "نص البلاغ طويل جداً (الحد 2000 حرف)");

                var driver = await _context.SaleMan.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SaleManId == saleManId);
                if (driver == null || driver.IsDelete == true)
                    return Response(false, "حساب المندوب غير موجود");
                if (driver.IsActive == false)
                    return Response(false, "حسابك غير نشط — لا يمكنك تقديم بلاغ");

                var order = await _context.Orders.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId);
                if (order == null)
                    return Response(false, "الطلب غير موجود");

                // Prefer assigned driver; allow report only on own assigned orders.
                if (order.SaleManId is null or 0 || order.SaleManId != saleManId)
                    return Response(false, "يمكنك الإبلاغ فقط عن طلبات مسندة إليك");

                var entity = new ProblemReport
                {
                    OrderId = dto.OrderId,
                    SaleManId = saleManId,
                    Message = message,
                    Status = ProblemReportStatus.Pending,
                    CreatedAt = Key.DateTimeIQ
                };

                await _context.ProblemReports.AddAsync(entity);
                await _context.SaveChangesAsync();

                return Response(true, "تم إرسال البلاغ بنجاح", new
                {
                    entity.ProblemReportId,
                    entity.OrderId,
                    order.OrderNo,
                    entity.Status,
                    StatusLabel = ProblemReportStatus.ToLabel(entity.Status),
                    entity.CreatedAt
                });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProblemReportsController => Submit");
                return Response(false, "حدث خطأ أثناء إرسال البلاغ");
            }
        }

        /// <summary>
        /// Driver: list own reports (optional).
        /// GET /problem-reports/mine
        /// </summary>
        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> Mine(int take = 50)
        {
            try
            {
                if (!IsDriver())
                    return Response(false, "غير مصرح");

                int saleManId = UserManager?.Id ?? 0;
                if (saleManId <= 0) return Response(false, "غير مصرح");
                if (take <= 0) take = 50;
                if (take > 200) take = 200;

                var raw = await (
                    from p in _context.ProblemReports.AsNoTracking()
                    where p.SaleManId == saleManId
                    join o in _context.Orders.AsNoTracking() on p.OrderId equals o.OrderId into oj
                    from o in oj.DefaultIfEmpty()
                    orderby p.CreatedAt descending
                    select new
                    {
                        p.ProblemReportId,
                        p.OrderId,
                        OrderNo = o != null ? (int?)o.OrderNo : null,
                        p.Message,
                        p.Status,
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.AdminNote
                    }
                ).Take(take).ToListAsync();

                var rows = raw.Select(x => new
                {
                    x.ProblemReportId,
                    x.OrderId,
                    x.OrderNo,
                    x.Message,
                    x.Status,
                    StatusLabel = ProblemReportStatus.ToLabel(x.Status),
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.AdminNote
                }).ToList();

                return Response(true, rows);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProblemReportsController => Mine");
                return Response(false, "حدث خطأ");
            }
        }

        /// <summary>
        /// Admin list with filters. Newest first.
        /// GET /problem-reports?status=&orderNo=&driverName=&dateFrom=&dateTo=&take=
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> List(
            int? status = null,
            string? orderNo = null,
            string? driverName = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int take = 200)
        {
            try
            {
                if (!IsAdmin())
                    return Response(false, "غير مصرح");

                if (take <= 0) take = 200;
                if (take > 500) take = 500;

                var query =
                    from p in _context.ProblemReports.AsNoTracking()
                    join o in _context.Orders.AsNoTracking() on p.OrderId equals o.OrderId into oj
                    from o in oj.DefaultIfEmpty()
                    join s in _context.SaleMan.AsNoTracking() on p.SaleManId equals s.SaleManId into sj
                    from s in sj.DefaultIfEmpty()
                    join r in _context.Restaurant.AsNoTracking() on (o != null ? o.RestaurantId : 0) equals r.RestaurantId into rj
                    from r in rj.DefaultIfEmpty()
                    select new { p, o, s, r };

                if (status.HasValue)
                    query = query.Where(x => x.p.Status == status.Value);

                if (!string.IsNullOrWhiteSpace(orderNo))
                {
                    var no = orderNo.Trim();
                    if (int.TryParse(no, out int orderNoInt))
                        query = query.Where(x => x.o != null && x.o.OrderNo == orderNoInt);
                }

                if (!string.IsNullOrWhiteSpace(driverName))
                {
                    var name = driverName.Trim();
                    query = query.Where(x => x.s != null && x.s.Name != null && x.s.Name.Contains(name));
                }

                if (dateFrom.HasValue)
                    query = query.Where(x => x.p.CreatedAt >= dateFrom.Value.Date);

                if (dateTo.HasValue)
                {
                    var end = dateTo.Value.Date.AddDays(1);
                    query = query.Where(x => x.p.CreatedAt < end);
                }

                var raw = await query
                    .OrderByDescending(x => x.p.CreatedAt)
                    .ThenByDescending(x => x.p.ProblemReportId)
                    .Take(take)
                    .Select(x => new
                    {
                        x.p.ProblemReportId,
                        x.p.OrderId,
                        OrderNo = x.o != null ? (int?)x.o.OrderNo : null,
                        OrderDate = x.o != null ? (DateTime?)x.o.OrderDate : null,
                        x.p.SaleManId,
                        SaleManName = x.s != null ? x.s.Name : null,
                        SaleManPhone = x.s != null ? x.s.Phone : null,
                        RestaurantName = x.r != null ? x.r.Name : null,
                        x.p.Message,
                        x.p.Status,
                        x.p.CreatedAt,
                        x.p.UpdatedAt,
                        x.p.AdminNote
                    })
                    .ToListAsync();

                var rows = raw.Select(x => new
                {
                    x.ProblemReportId,
                    x.OrderId,
                    x.OrderNo,
                    x.OrderDate,
                    x.SaleManId,
                    x.SaleManName,
                    x.SaleManPhone,
                    x.RestaurantName,
                    x.Message,
                    x.Status,
                    StatusLabel = ProblemReportStatus.ToLabel(x.Status),
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.AdminNote
                }).ToList();

                return Response(true, rows);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProblemReportsController => List");
                return Response(false, "حدث خطأ أثناء جلب البلاغات");
            }
        }

        /// <summary>
        /// Admin dashboard stats.
        /// GET /problem-reports/stats?dateFrom=&dateTo=
        /// </summary>
        [Authorize]
        [HttpGet("stats")]
        public async Task<IActionResult> Stats(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                if (!IsAdmin())
                    return Response(false, "غير مصرح");

                var query = _context.ProblemReports.AsNoTracking().AsQueryable();

                if (dateFrom.HasValue)
                    query = query.Where(p => p.CreatedAt >= dateFrom.Value.Date);
                if (dateTo.HasValue)
                {
                    var end = dateTo.Value.Date.AddDays(1);
                    query = query.Where(p => p.CreatedAt < end);
                }

                var groups = await query
                    .GroupBy(p => p.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                int CountOf(int s) => groups.FirstOrDefault(g => g.Status == s)?.Count ?? 0;

                return Response(true, new
                {
                    total = groups.Sum(g => g.Count),
                    pending = CountOf(ProblemReportStatus.Pending),
                    inProgress = CountOf(ProblemReportStatus.InProgress),
                    resolved = CountOf(ProblemReportStatus.Resolved),
                    unresolved = CountOf(ProblemReportStatus.Unresolved)
                });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProblemReportsController => Stats");
                return Response(false, "حدث خطأ");
            }
        }

        /// <summary>
        /// Admin: change report status.
        /// POST /problem-reports/{id}/status
        /// Body: { status: 0|1|2|3, adminNote?: string }
        /// </summary>
        [Authorize]
        [HttpPost("{id:int}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] SetStatusDto? dto)
        {
            try
            {
                if (!IsAdmin())
                    return Response(false, "غير مصرح");

                if (dto == null || !ProblemReportStatus.IsValid(dto.Status))
                    return Response(false, "حالة غير صالحة");

                var row = await _context.ProblemReports.FirstOrDefaultAsync(p => p.ProblemReportId == id);
                if (row == null)
                    return Response(false, "البلاغ غير موجود");

                row.Status = dto.Status;
                row.UpdatedAt = Key.DateTimeIQ;
                if (dto.AdminNote != null)
                    row.AdminNote = string.IsNullOrWhiteSpace(dto.AdminNote) ? null : dto.AdminNote.Trim();

                _context.Entry(row).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Response(true, "تم تحديث حالة البلاغ", new
                {
                    row.ProblemReportId,
                    row.Status,
                    StatusLabel = ProblemReportStatus.ToLabel(row.Status),
                    row.UpdatedAt,
                    row.AdminNote
                });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ProblemReportsController => SetStatus");
                return Response(false, "حدث خطأ أثناء تحديث الحالة");
            }
        }

        public class SubmitProblemReportDto
        {
            public int OrderId { get; set; }
            public string? Message { get; set; }
        }

        public class SetStatusDto
        {
            public int Status { get; set; }
            public string? AdminNote { get; set; }
        }
    }
}
