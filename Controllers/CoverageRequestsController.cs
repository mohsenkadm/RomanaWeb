using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    [Route("coverage-requests")]
    public class CoverageRequestsController : MasterController
    {
        private readonly DB_Context _context;
        private readonly ILoggerRepository _logger;

        public CoverageRequestsController(DB_Context context, ILoggerRepository logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool IsAdmin() =>
            UserManager != null && string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] CoverageRequestDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return Response(false, "الاسم مطلوب");
                if (string.IsNullOrWhiteSpace(dto.Phone))
                    return Response(false, "رقم الهاتف مطلوب");
                if (dto.Lat == 0 || dto.Lng == 0)
                    return Response(false, "الموقع مطلوب");

                var entity = new ServiceCoverageRequest
                {
                    Name = dto.Name.Trim(),
                    Phone = dto.Phone.Trim(),
                    Address = dto.Address?.Trim(),
                    Lat = dto.Lat,
                    Lng = dto.Lng,
                    CreatedAt = Key.DateTimeIQ,
                    IsProcessed = false
                };
                await _context.ServiceCoverageRequests.AddAsync(entity);
                await _context.SaveChangesAsync();
                return Response(true, "تم تسجيل طلب توفير الخدمة", new { entity.ServiceCoverageRequestId });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CoverageRequestsController => Submit");
                return Response(false, "حدث خطأ");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> List(bool? processed = null, string? phone = null, string? name = null, int take = 200)
        {
            try
            {
                if (!IsAdmin())
                    return Response(false, "غير مصرح");

                if (take <= 0) take = 200;
                if (take > 500) take = 500;

                var query = _context.ServiceCoverageRequests.AsNoTracking().AsQueryable();

                if (processed.HasValue)
                    query = query.Where(r => r.IsProcessed == processed.Value);

                if (!string.IsNullOrWhiteSpace(phone))
                    query = query.Where(r => r.Phone.Contains(phone.Trim()));

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(r => r.Name.Contains(name.Trim()));

                var rows = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(take)
                    .ToListAsync();

                return Response(true, rows);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CoverageRequestsController => List");
                return Response(false, "خطأ");
            }
        }

        [Authorize]
        [HttpPost("{id:int}/processed")]
        public async Task<IActionResult> MarkProcessed(int id, [FromBody] MarkProcessedDto? dto)
        {
            try
            {
                if (!IsAdmin())
                    return Response(false, "غير مصرح");

                var row = await _context.ServiceCoverageRequests.FirstOrDefaultAsync(r => r.ServiceCoverageRequestId == id);
                if (row == null)
                    return Response(false, "الطلب غير موجود");

                row.IsProcessed = dto?.IsProcessed ?? true;
                _context.Entry(row).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Response(true, row.IsProcessed ? "تم تحديد الطلب كمعالج" : "تم إرجاع الطلب كغير معالج", row);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "CoverageRequestsController => MarkProcessed");
                return Response(false, "حدث خطأ");
            }
        }

        public class MarkProcessedDto
        {
            public bool IsProcessed { get; set; } = true;
        }

        public class CoverageRequestDto
        {
            public string? Name { get; set; }
            public string? Phone { get; set; }
            public string? Address { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
    }
}
