using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    [Authorize]
    [Route("zones")]
    public class ZonesController : MasterController
    {
        private readonly ILoggerRepository _logger;
        private readonly DB_Context _context;
        private readonly IPricingService _pricing;
        private readonly IRoutingService _routing;

        public ZonesController(ILoggerRepository logger, DB_Context context, IPricingService pricing, IRoutingService routing)
        {
            _logger = logger;
            _context = context;
            _pricing = pricing;
            _routing = routing;
        }

        private bool IsAdmin() =>
            UserManager != null && string.Equals(UserManager.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var zones = await _context.Zone.AsNoTracking().OrderBy(z => z.Name).ToListAsync();
                return Response(true, zones);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => List"); return Response(false, "خطأ"); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var zone = await _context.Zone.AsNoTracking().FirstOrDefaultAsync(z => z.ZoneId == id);
                if (zone == null) return Response(false, "المنطقة غير موجودة");
                return Response(true, zone);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => Get"); return Response(false, "خطأ"); }
        }

        [HttpGet("{id:int}/geojson")]
        public async Task<IActionResult> GeoJson(int id)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var zone = await _context.Zone.AsNoTracking().FirstOrDefaultAsync(z => z.ZoneId == id);
                if (zone == null) return Response(false, "المنطقة غير موجودة");
                return Response(true, new { zone.ZoneId, zone.Name, geoJson = zone.GeoJson });
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => GeoJson"); return Response(false, "خطأ"); }
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
                foreach (var row in sheet.RowsUsed().Skip(1))
                {
                    string name = row.Cell(1).GetString().Trim();
                    string geo = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(geo)) continue;

                    decimal? basePrice = ParseDecimal(row.Cell(3).GetString());
                    decimal lza = ParseDecimal(row.Cell(4).GetString()) ?? 3m;
                    decimal ecaPrice = ParseDecimal(row.Cell(5).GetString()) ?? 250m;
                    decimal maxEca = ParseDecimal(row.Cell(6).GetString()) ?? 2500m;
                    decimal? nearPrice = ParseDecimal(row.Cell(7).GetString());

                    var existing = await _context.Zone.FirstOrDefaultAsync(z => z.Name == name);
                    if (existing == null)
                    {
                        await _context.Zone.AddAsync(new Zone
                        {
                            Name = name,
                            GeoJson = geo,
                            IsActive = true,
                            BaseDeliveryPrice = basePrice,
                            LzaKm = lza,
                            EcaPricePerKm = ecaPrice,
                            MaxEcaFee = maxEca,
                            NearRestaurantPrice = nearPrice
                        });
                        added++;
                    }
                    else
                    {
                        existing.GeoJson = geo;
                        existing.IsActive = true;
                        existing.BaseDeliveryPrice = basePrice;
                        existing.LzaKm = lza;
                        existing.EcaPricePerKm = ecaPrice;
                        existing.MaxEcaFee = maxEca;
                        existing.NearRestaurantPrice = nearPrice;
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
                        await _context.ZonePrice.AddAsync(new ZonePrice { FromZoneId = fromId, ToZoneId = toId, Price = price });
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
                return Response(false, "حدث خطأ");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateZone([FromBody] ZoneUpsertRequest request)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var validation = ValidateZoneRequest(request);
                if (validation != null) return Response(false, validation);

                var existing = await _context.Zone.FirstOrDefaultAsync(z => z.Name == request.Name!.Trim());
                if (existing != null)
                {
                    ApplyZoneFields(existing, request);
                    _context.Entry(existing).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Response(true, "تم تحديث المنطقة بنجاح", existing);
                }

                var zone = new Zone();
                ApplyZoneFields(zone, request);
                await _context.Zone.AddAsync(zone);
                await _context.SaveChangesAsync();
                return Response(true, "تم اضافة المنطقة بنجاح", zone);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => CreateZone");
                return Response(false, "حدث خطأ اثناء الحفظ");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateZone(int id, [FromBody] ZoneUpsertRequest request)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var zone = await _context.Zone.FirstOrDefaultAsync(z => z.ZoneId == id);
                if (zone == null) return Response(false, "المنطقة غير موجودة");
                var validation = ValidateZoneRequest(request);
                if (validation != null) return Response(false, validation);

                ApplyZoneFields(zone, request);
                _context.Entry(zone).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Response(true, "تم التحديث", zone);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => UpdateZone");
                return Response(false, "خطأ");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteZone(int id)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var zone = await _context.Zone.FirstOrDefaultAsync(z => z.ZoneId == id);
                if (zone == null) return Response(false, "غير موجود");
                zone.IsActive = false;
                _context.Entry(zone).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Response(true, "تم التعطيل");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => DeleteZone");
                return Response(false, "خطأ");
            }
        }

        [HttpPost("route")]
        public async Task<IActionResult> GetRoute([FromBody] RouteRequest request)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                if (request == null) return Response(false, "بيانات غير صالحة");

                var route = await _routing.GetRouteDistanceKmAsync(
                    request.FromLat, request.FromLng, request.ToLat, request.ToLng);

                return Response(true, new
                {
                    distanceKm = route.DistanceKm,
                    source = route.Source,
                    path = route.Path?.Select(p => new { lat = p.Lat, lng = p.Lng }).ToList()
                });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => GetRoute");
                return Response(false, "تعذر حساب المسار");
            }
        }

        [HttpPost("simulate")]
        public async Task<IActionResult> SimulatePricing([FromBody] QuoteRequest request)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                request ??= new QuoteRequest();
                request.ForceZonePricing = true;
                var res = await _pricing.Quote(request);
                if (!res.success)
                    return Response(false, res.msg ?? "تعذر الحساب");
                return Response(true, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => SimulatePricing");
                return Response(false, "خطأ");
            }
        }

        [HttpPost("matrix/create")]
        public async Task<IActionResult> CreateMatrixEntry([FromBody] CreateMatrixRequest request)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                if (request.FromZoneId <= 0 || request.ToZoneId <= 0)
                    return Response(false, "رجاءا اختر منطقة المصدر والوجهة");
                if (request.Price < 0)
                    return Response(false, "السعر غير صالح");

                var existing = await _context.ZonePrice
                    .FirstOrDefaultAsync(p => p.FromZoneId == request.FromZoneId && p.ToZoneId == request.ToZoneId);
                if (existing == null)
                {
                    var entry = new ZonePrice
                    {
                        FromZoneId = request.FromZoneId,
                        ToZoneId = request.ToZoneId,
                        Price = request.Price
                    };
                    await _context.ZonePrice.AddAsync(entry);
                    await _context.SaveChangesAsync();
                    return Response(true, "تم اضافة السعر بنجاح", entry);
                }

                existing.Price = request.Price;
                _context.Entry(existing).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Response(true, "تم تحديث السعر بنجاح", existing);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => CreateMatrixEntry");
                return Response(false, "خطأ");
            }
        }

        [HttpDelete("matrix/{id:int}")]
        public async Task<IActionResult> DeleteMatrixEntry(int id)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var entry = await _context.ZonePrice.FirstOrDefaultAsync(p => p.ZonePriceId == id);
                if (entry == null) return Response(false, "السعر غير موجود");
                _context.ZonePrice.Remove(entry);
                await _context.SaveChangesAsync();
                return Response(true, "تم الحذف");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => DeleteMatrixEntry");
                return Response(false, "خطأ");
            }
        }

        [HttpGet("matrix")]
        public async Task<IActionResult> Matrix()
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var zoneNames = await _context.Zone.AsNoTracking()
                    .ToDictionaryAsync(z => z.ZoneId, z => z.Name);
                var prices = await _context.ZonePrice.AsNoTracking().ToListAsync();
                var data = prices.Select(p => new
                {
                    p.ZonePriceId,
                    p.FromZoneId,
                    p.ToZoneId,
                    p.Price,
                    FromZoneName = zoneNames.TryGetValue(p.FromZoneId, out var fromName) ? fromName : "-",
                    ToZoneName = zoneNames.TryGetValue(p.ToZoneId, out var toName) ? toName : "-"
                }).ToList();
                return Response(true, data);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => Matrix"); return Response(false, "خطأ"); }
        }

        [HttpGet("links-summary")]
        public async Task<IActionResult> LinksSummary()
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");

                var zones = await _context.Zone.AsNoTracking()
                    .Where(z => z.IsActive)
                    .OrderBy(z => z.Name)
                    .Select(z => new { z.ZoneId, z.Name })
                    .ToListAsync();

                var byRestaurant = await _context.RestaurantZone.AsNoTracking()
                    .GroupBy(rz => rz.RestaurantId)
                    .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ZoneId).ToList());

                var bySaleMan = await _context.SaleManZone.AsNoTracking()
                    .GroupBy(sz => sz.SaleManId)
                    .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ZoneId).ToList());

                return Response(true, new { zones, byRestaurant, bySaleMan });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => LinksSummary");
                return Response(false, "خطأ");
            }
        }

        [HttpGet("restaurant/{restaurantId:int}")]
        public async Task<IActionResult> GetRestaurantZones(int restaurantId)
        {
            try
            {
                var ids = await _context.RestaurantZone.AsNoTracking()
                    .Where(rz => rz.RestaurantId == restaurantId)
                    .Select(rz => rz.ZoneId)
                    .ToListAsync();
                return Response(true, ids);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => GetRestaurantZones"); return Response(false, "خطأ"); }
        }

        [HttpPut("restaurant/{restaurantId:int}")]
        public async Task<IActionResult> SetRestaurantZones(int restaurantId, [FromBody] int[] zoneIds)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var existing = await _context.RestaurantZone.Where(rz => rz.RestaurantId == restaurantId).ToListAsync();
                _context.RestaurantZone.RemoveRange(existing);
                foreach (var zid in zoneIds ?? Array.Empty<int>())
                {
                    if (zid > 0)
                        await _context.RestaurantZone.AddAsync(new RestaurantZone { RestaurantId = restaurantId, ZoneId = zid });
                }
                await _context.SaveChangesAsync();
                return Response(true, "تم تحديث زونات المطعم");
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => SetRestaurantZones"); return Response(false, "خطأ"); }
        }

        [HttpGet("saleman/{saleManId:int}")]
        public async Task<IActionResult> GetSaleManZones(int saleManId)
        {
            try
            {
                if (!IsAdmin() && UserManager?.Id != saleManId)
                    return Response(false, "غير مصرح");
                var ids = await _context.SaleManZone.AsNoTracking()
                    .Where(sz => sz.SaleManId == saleManId)
                    .Select(sz => sz.ZoneId)
                    .ToListAsync();
                return Response(true, ids);
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => GetSaleManZones"); return Response(false, "خطأ"); }
        }

        [HttpPut("saleman/{saleManId:int}")]
        public async Task<IActionResult> SetSaleManZones(int saleManId, [FromBody] int[] zoneIds)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var existing = await _context.SaleManZone.Where(sz => sz.SaleManId == saleManId).ToListAsync();
                _context.SaleManZone.RemoveRange(existing);
                foreach (var zid in zoneIds ?? Array.Empty<int>())
                {
                    if (zid > 0)
                        await _context.SaleManZone.AddAsync(new SaleManZone { SaleManId = saleManId, ZoneId = zid });
                }
                await _context.SaveChangesAsync();
                return Response(true, "تم تحديث زونات المندوب");
            }
            catch (Exception ex) { await _logger.WriteAsync(ex, "ZonesController => SetSaleManZones"); return Response(false, "خطأ"); }
        }

        [HttpGet("system-settings")]
        public async Task<IActionResult> GetSystemSettings()
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var s = await _context.AppSettings.AsNoTracking().FirstOrDefaultAsync();
                if (s == null)
                {
                    s = new AppSettings { PricePerKm = 500, DefaultOrderCost = 3000, IqdRoundingStep = 250 };
                    await _context.AppSettings.AddAsync(s);
                    await _context.SaveChangesAsync();
                }
                return Response(true, new
                {
                    s.IqdRoundingStep,
                    s.AllowBusyDriverDispatch,
                    s.PricePerKm,
                    s.MinChargeKmThreshold,
                    s.MinChargeAmount,
                    s.RoundingMode
                });
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => GetSystemSettings");
                return Response(false, "خطأ");
            }
        }

        [HttpPut("system-settings")]
        public async Task<IActionResult> UpdateSystemSettings([FromBody] SystemSettingsRequest request)
        {
            try
            {
                if (!IsAdmin()) return Response(false, "غير مصرح");
                var s = await _context.AppSettings.FirstOrDefaultAsync();
                if (s == null)
                {
                    s = new AppSettings();
                    await _context.AppSettings.AddAsync(s);
                }
                s.IqdRoundingStep = request.IqdRoundingStep > 0 ? request.IqdRoundingStep : 250;
                s.AllowBusyDriverDispatch = request.AllowBusyDriverDispatch;
                s.PricePerKm = request.PricePerKm > 0 ? request.PricePerKm : 500;
                s.MinChargeKmThreshold = request.MinChargeKmThreshold >= 0 ? request.MinChargeKmThreshold : 1.5m;
                s.MinChargeAmount = request.MinChargeAmount >= 0 ? request.MinChargeAmount : 500m;
                s.RoundingMode = string.IsNullOrWhiteSpace(request.RoundingMode) ? "Ceil" : request.RoundingMode.Trim();
                await _context.SaveChangesAsync();
                return Response(true, "تم حفظ إعدادات النظام");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "ZonesController => UpdateSystemSettings");
                return Response(false, "خطأ");
            }
        }

        [AllowAnonymous]
        [HttpGet("template/zones")]
        public IActionResult DownloadZonesTemplate()
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Zones");
            ws.Cell(1, 1).Value = "Name";
            ws.Cell(1, 2).Value = "GeoJsonPolygon";
            ws.Cell(1, 3).Value = "BaseDeliveryPrice";
            ws.Cell(1, 4).Value = "LzaKm";
            ws.Cell(1, 5).Value = "EcaPricePerKm";
            ws.Cell(1, 6).Value = "MaxEcaFee";
            ws.Cell(1, 7).Value = "NearRestaurantPrice";
            ws.Range(1, 1, 1, 7).Style.Font.Bold = true;

            ws.Cell(2, 1).Value = "قضاء المدينة";
            ws.Cell(2, 2).Value = "{\"type\":\"Polygon\",\"coordinates\":[[[47.75,30.48],[47.82,30.48],[47.82,30.52],[47.75,30.52],[47.75,30.48]]]}";
            ws.Cell(2, 3).Value = 3000; ws.Cell(2, 4).Value = 3; ws.Cell(2, 5).Value = 250; ws.Cell(2, 6).Value = 2500; ws.Cell(2, 7).Value = 1500;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "zones_template.xlsx");
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
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Cell(2, 1).Value = "قضاء المدينة"; ws.Cell(2, 2).Value = "الامام الصادق"; ws.Cell(2, 3).Value = 3000;
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "zones_matrix_template.xlsx");
        }

        private static decimal? ParseDecimal(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return decimal.TryParse(s.Trim(), out decimal v) ? v : null;
        }

        private static string? ValidateZoneRequest(ZoneUpsertRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return "رجاءا ادخل اسم المنطقة";
            if (string.IsNullOrWhiteSpace(request.GeoJson))
                return "رجاءا ادخل GeoJSON";
            if (!ZoneGeometryHelper.TryNormalizeToPolygonGeoJson(request.GeoJson.Trim(), out var normalized))
                return "GeoJSON غير صالح — تأكد من Polygon وترتيب [lng,lat]";
            request.GeoJson = normalized;
            return null;
        }

        private static void ApplyZoneFields(Zone zone, ZoneUpsertRequest request)
        {
            zone.Name = request.Name!.Trim();
            zone.GeoJson = request.GeoJson!.Trim();
            zone.IsActive = request.IsActive;
            zone.BaseDeliveryPrice = request.BaseDeliveryPrice;
            zone.LzaKm = request.LzaKm > 0 ? request.LzaKm : 3m;
            zone.EcaPricePerKm = request.EcaPricePerKm > 0 ? request.EcaPricePerKm : 250m;
            zone.MaxEcaFee = request.MaxEcaFee > 0 ? request.MaxEcaFee : 2500m;
            zone.MaxTotalDeliveryFee = request.MaxTotalDeliveryFee > 0 ? request.MaxTotalDeliveryFee : null;
            zone.NearRestaurantPrice = request.NearRestaurantPrice;
            zone.NearRestaurantKm = request.NearRestaurantKm > 0 ? request.NearRestaurantKm : 1m;
        }

        public class ZoneUpsertRequest
        {
            public string? Name { get; set; }
            public string? GeoJson { get; set; }
            public bool IsActive { get; set; } = true;
            public decimal? BaseDeliveryPrice { get; set; }
            public decimal LzaKm { get; set; } = 3m;
            public decimal EcaPricePerKm { get; set; } = 250m;
            public decimal MaxEcaFee { get; set; } = 2500m;
            public decimal? MaxTotalDeliveryFee { get; set; }
            public decimal? NearRestaurantPrice { get; set; }
            public decimal NearRestaurantKm { get; set; } = 1m;
        }

        public class CreateMatrixRequest
        {
            public int FromZoneId { get; set; }
            public int ToZoneId { get; set; }
            public decimal Price { get; set; }
        }

        public class RouteRequest
        {
            public double FromLat { get; set; }
            public double FromLng { get; set; }
            public double ToLat { get; set; }
            public double ToLng { get; set; }
        }

        public class SystemSettingsRequest
        {
            public int IqdRoundingStep { get; set; } = 250;
            public bool AllowBusyDriverDispatch { get; set; }
            public decimal PricePerKm { get; set; } = 500m;
            public decimal MinChargeKmThreshold { get; set; } = 1.5m;
            public decimal MinChargeAmount { get; set; } = 500m;
            public string RoundingMode { get; set; } = "Ceil";
        }
    }
}
