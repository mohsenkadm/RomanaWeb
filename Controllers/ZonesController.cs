using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    // Section 2.2 - Zones (admin only).
    // Excel format expected (first sheet):
    //   Column A: Zone Name
    //   Column B: GeoJSON Polygon (single-line). Example:
    //     {"type":"Polygon","coordinates":[[[44.36,33.31],[44.40,33.31],[44.40,33.34],[44.36,33.34],[44.36,33.31]]]}
    //
    // Zone-to-zone matrix Excel (second optional endpoint):
    //   Column A: From Zone Name
    //   Column B: To Zone Name
    //   Column C: Price (IQD)
    [Authorize]
    [Route("zones")]
    public class ZonesController : MasterController
    {
        private readonly ILoggerRepository _logger;
        private readonly DB_Context _context;

        public ZonesController(ILoggerRepository logger, DB_Context context)
        {
            _logger = logger;
            _context = context;
        }

        private bool IsAdmin() =>
            UserManager != null && string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var zones = await _context.Zone.AsNoTracking().ToListAsync();
                return Response(true, zones);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => List"); return Response(false, "خطأ"); }
        }

        [HttpPost("upload")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadZones(IFormFile file)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                if (file == null || file.Length == 0) return Response(false, "لم يتم اختيار ملف");

                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var sheet = workbook.Worksheets.First();

                int added = 0, updated = 0;
                foreach (var row in sheet.RowsUsed().Skip(1)) // skip header
                {
                    string name = row.Cell(1).GetString().Trim();
                    string geo = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(geo)) continue;

                    var existing = await _context.Zone.FirstOrDefaultAsync(z => z.Name == name);
                    if (existing == null)
                    {
                        await _context.Zone.AddAsync(new Zone { Name = name, GeoJson = geo, IsActive = true });
                        added++;
                    }
                    else
                    {
                        existing.GeoJson = geo;
                        existing.IsActive = true;
                        _context.Entry(existing).State = EntityState.Modified;
                        updated++;
                    }
                }
                await _context.SaveChangesAsync();
                return Response(true, $"تم استيراد المناطق. أضيفت: {added}, حُدّثت: {updated}");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => UploadZones");
                return Response(false, "حدث خطأ اثناء قراءة الملف");
            }
        }

        [HttpPost("matrix/upload")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadMatrix(IFormFile file)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                if (file == null || file.Length == 0) return Response(false, "لم يتم اختيار ملف");

                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var sheet = workbook.Worksheets.First();

                var zones = await _context.Zone.AsNoTracking().ToListAsync();
                var byName = zones.ToDictionary(z => z.Name, z => z.ZoneId, StringComparer.OrdinalIgnoreCase);

                int added = 0, updated = 0, skipped = 0;
                foreach (var row in sheet.RowsUsed().Skip(1))
                {
                    string from = row.Cell(1).GetString().Trim();
                    string to = row.Cell(2).GetString().Trim();
                    if (!byName.TryGetValue(from, out int fromId) || !byName.TryGetValue(to, out int toId))
                    { skipped++; continue; }
                    if (!decimal.TryParse(row.Cell(3).GetString().Trim(), out decimal price))
                    { skipped++; continue; }

                    var existing = await _context.ZonePrice
                        .FirstOrDefaultAsync(p => p.FromZoneId == fromId && p.ToZoneId == toId);
                    if (existing == null)
                    {
                        await _context.ZonePrice.AddAsync(new ZonePrice
                        {
                            FromZoneId = fromId,
                            ToZoneId = toId,
                            Price = price
                        });
                        added++;
                    }
                    else
                    {
                        existing.Price = price;
                        _context.Entry(existing).State = EntityState.Modified;
                        updated++;
                    }
                }
                await _context.SaveChangesAsync();
                return Response(true, $"تم استيراد مصفوفة الأسعار. أضيفت: {added}, حُدّثت: {updated}, تخطّيت: {skipped}");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => UploadMatrix");
                return Response(false, "حدث خطأ اثناء قراءة الملف");
            }
        }

        [HttpGet("matrix")]
        public async Task<IActionResult> Matrix()
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var data = await _context.ZonePrice.AsNoTracking().ToListAsync();
                return Response(true, data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => Matrix"); return Response(false, "خطأ"); }
        }

        // --------------------------------------------------------------------
        // Excel template downloads (Section 2.2).
        // [AllowAnonymous] so the browser can download via a plain anchor tag
        // (no JWT header). Templates contain no sensitive data.
        // --------------------------------------------------------------------

        [AllowAnonymous]
        [HttpGet("template/zones")]
        public IActionResult DownloadZonesTemplate()
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Zones");

            ws.Cell(1, 1).Value = "Name";
            ws.Cell(1, 2).Value = "GeoJsonPolygon";
            var header = ws.Range(1, 1, 1, 2);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.Cell(2, 1).Value = "Karada";
            ws.Cell(2, 2).Value =
                "{\"type\":\"Polygon\",\"coordinates\":[[[44.40,33.30],[44.45,33.30],[44.45,33.34],[44.40,33.34],[44.40,33.30]]]}";

            ws.Cell(3, 1).Value = "Mansour";
            ws.Cell(3, 2).Value =
                "{\"type\":\"Polygon\",\"coordinates\":[[[44.32,33.30],[44.38,33.30],[44.38,33.34],[44.32,33.34],[44.32,33.30]]]}";

            ws.Cell(4, 1).Value = "Karkh";
            ws.Cell(4, 2).Value =
                "{\"type\":\"Polygon\",\"coordinates\":[[[44.36,33.32],[44.40,33.32],[44.40,33.36],[44.36,33.36],[44.36,33.32]]]}";

            ws.Cell(1, 4).Value = "Notes:";
            ws.Cell(2, 4).Value = "Column A = zone name (unique)";
            ws.Cell(3, 4).Value = "Column B = GeoJSON Polygon, single line, [lng, lat] order";
            ws.Cell(4, 4).Value = "Re-uploading the same name updates the polygon";

            ws.Column(1).Width = 22;
            ws.Column(2).Width = 110;
            ws.Column(4).Width = 60;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "zones_template.xlsx");
        }

        [AllowAnonymous]
        [HttpGet("template/matrix")]
        public IActionResult DownloadMatrixTemplate()
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Matrix");

            ws.Cell(1, 1).Value = "FromZoneName";
            ws.Cell(1, 2).Value = "ToZoneName";
            ws.Cell(1, 3).Value = "PriceIQD";
            var header = ws.Range(1, 1, 1, 3);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.Cell(2, 1).Value = "Karada"; ws.Cell(2, 2).Value = "Mansour"; ws.Cell(2, 3).Value = 4000;
            ws.Cell(3, 1).Value = "Mansour"; ws.Cell(3, 2).Value = "Karada"; ws.Cell(3, 3).Value = 4000;
            ws.Cell(4, 1).Value = "Karada"; ws.Cell(4, 2).Value = "Karkh"; ws.Cell(4, 3).Value = 3500;
            ws.Cell(5, 1).Value = "Karkh"; ws.Cell(5, 2).Value = "Karada"; ws.Cell(5, 3).Value = 3500;
            ws.Cell(6, 1).Value = "Mansour"; ws.Cell(6, 2).Value = "Karkh"; ws.Cell(6, 3).Value = 3000;
            ws.Cell(7, 1).Value = "Karkh"; ws.Cell(7, 2).Value = "Mansour"; ws.Cell(7, 3).Value = 3000;

            ws.Cell(1, 5).Value = "Notes:";
            ws.Cell(2, 5).Value = "Column A = source zone name (must already exist)";
            ws.Cell(3, 5).Value = "Column B = destination zone name (must already exist)";
            ws.Cell(4, 5).Value = "Column C = delivery price in IQD";
            ws.Cell(5, 5).Value = "Re-uploading the same From/To pair updates the price";

            ws.Column(1).Width = 22;
            ws.Column(2).Width = 22;
            ws.Column(3).Width = 14;
            ws.Column(5).Width = 60;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "zones_matrix_template.xlsx");
        }
    }
}
