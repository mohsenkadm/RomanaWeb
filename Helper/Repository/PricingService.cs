using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    /// <summary>
    /// Section 2 - Single pricing engine used by Customer/Merchant/Driver apps.
    /// </summary>
    public class PricingService : IPricingService, IRegisterScopped
    {
        private readonly DB_Context _context;
        private readonly IDistanceService _distance;

        public PricingService(DB_Context context, IDistanceService distance)
        {
            _context = context;
            _distance = distance;
        }

        public async Task<ResObj> Quote(QuoteRequest request)
        {
            if (request == null)
                return Result.Return(false, "invalid request");

            var settings = await _context.AppSettings.AsNoTracking().FirstOrDefaultAsync()
                           ?? new AppSettings
                           {
                               PricePerKm = 500,
                               DefaultOrderCost = 3000,
                               MinChargeKmThreshold = 1.5m,
                               MinChargeAmount = 500m,
                               RoundingMode = "Ceil"
                           };

            double distanceKm;
            if (request.DistanceKm.HasValue)
            {
                distanceKm = request.DistanceKm.Value;
            }
            else
            {
                if (!_distance.IsValidCoord(request.PickupLat, request.PickupLng) ||
                    !_distance.IsValidCoord(request.DropoffLat, request.DropoffLng))
                    return Result.Return(false, "يجب تحديد إحداثيات الاستلام والتسليم");

                distanceKm = _distance.HaversineKm(
                    request.PickupLat, request.PickupLng,
                    request.DropoffLat, request.DropoffLng);
            }

            distanceKm = _distance.RoundKm(distanceKm);

            if (distanceKm <= 0)
                return Result.Return(false, "المسافة غير صالحة");

            var cityResult = await TryCityPricing(request, settings, distanceKm);
            if (cityResult != null)
                return Result.Return(true, cityResult);

            var response = new QuoteResponse
            {
                DistanceKm = distanceKm,
                PricePerKm = settings.PricePerKm,
                MinimumCharge = settings.MinChargeAmount
            };

            if ((decimal)distanceKm < settings.MinChargeKmThreshold)
            {
                response.MinChargeApplied = true;
                response.DistanceFee = 0;
                response.ZoneFee = 0;
                response.Total = settings.MinChargeAmount;
                response.PricingSource = "minimum";
                return Result.Return(true, response);
            }

            var zoneResult = await TryZonePricing(request, settings, distanceKm);
            if (zoneResult != null)
                return Result.Return(true, zoneResult);

            decimal billableKm = ApplyRounding((decimal)distanceKm, settings.RoundingMode);
            response.DistanceFee = billableKm * settings.PricePerKm;
            response.ZoneFee = 0;
            response.Total = response.DistanceFee;
            response.PricingSource = "distance";
            return Result.Return(true, response);
        }

        private async Task<QuoteResponse?> TryCityPricing(QuoteRequest request, AppSettings settings, double distanceKm)
        {
            if (request.RestaurantId is not > 0 || request.CityId is not > 0)
                return null;

            var cityRow = await _context.RestaurantCity.AsNoTracking()
                .FirstOrDefaultAsync(rc => rc.RestaurantId == request.RestaurantId && rc.CityId == request.CityId);
            if (cityRow?.CostDelivery is not > 0)
                return null;

            return new QuoteResponse
            {
                DistanceKm = distanceKm,
                PricePerKm = settings.PricePerKm,
                MinimumCharge = settings.MinChargeAmount,
                CityFee = cityRow.CostDelivery.Value,
                Total = cityRow.CostDelivery.Value,
                PricingSource = "city"
            };
        }

        private async Task<QuoteResponse?> TryZonePricing(QuoteRequest request, AppSettings settings, double distanceKm)
        {
            var zones = await _context.Zone.AsNoTracking().Where(z => z.IsActive).ToListAsync();
            if (zones.Count == 0) return null;

            int? pickupZone = ResolveZone(zones, request.PickupLng, request.PickupLat);
            int? dropoffZone = ResolveZone(zones, request.DropoffLng, request.DropoffLat);
            if (pickupZone == null || dropoffZone == null) return null;

            string? fromName = zones.FirstOrDefault(z => z.ZoneId == pickupZone)?.Name;
            string? toName = zones.FirstOrDefault(z => z.ZoneId == dropoffZone)?.Name;

            if (pickupZone.Value == dropoffZone.Value) return null;

            var entry = await _context.ZonePrice.AsNoTracking()
                .FirstOrDefaultAsync(p => p.FromZoneId == pickupZone.Value && p.ToZoneId == dropoffZone.Value);
            decimal zoneFee = entry?.Price ?? 0m;

            decimal billable = ApplyRounding((decimal)distanceKm, settings.RoundingMode);
            if (settings.ZoneMinKm > 0 && billable < settings.ZoneMinKm) billable = settings.ZoneMinKm;
            if (settings.ZoneMaxKm > 0 && billable > settings.ZoneMaxKm) billable = settings.ZoneMaxKm;
            decimal perKm = billable * settings.PricePerKm;

            return new QuoteResponse
            {
                DistanceKm = distanceKm,
                PricePerKm = settings.PricePerKm,
                DistanceFee = perKm,
                ZoneFee = zoneFee,
                MinimumCharge = settings.MinChargeAmount,
                Total = zoneFee + perKm,
                FromZone = fromName,
                ToZone = toName,
                MinChargeApplied = false,
                PricingSource = "zone"
            };
        }

        private static int? ResolveZone(List<Zone> zones, double lng, double lat)
        {
            foreach (var z in zones)
            {
                try
                {
                    var poly = JObject.Parse(z.GeoJson);
                    var coords = poly["coordinates"] as JArray;
                    if (coords == null || coords.Count == 0) continue;
                    var ring = coords[0] as JArray;
                    if (ring == null) continue;

                    var pts = ring.Select(p =>
                    {
                        var arr = (JArray)p;
                        return (X: (double)arr[0]!, Y: (double)arr[1]!);
                    }).ToList();
                    if (PointInPolygon(pts, lng, lat)) return z.ZoneId;
                }
                catch { /* ignore malformed geo */ }
            }
            return null;
        }

        private static bool PointInPolygon(List<(double X, double Y)> polygon, double x, double y)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];
                bool intersect = ((pi.Y > y) != (pj.Y > y)) &&
                                 (x < (pj.X - pi.X) * (y - pi.Y) / ((pj.Y - pi.Y) == 0 ? 1e-12 : (pj.Y - pi.Y)) + pi.X);
                if (intersect) inside = !inside;
            }
            return inside;
        }

        private static decimal ApplyRounding(decimal km, string mode)
        {
            if (string.Equals(mode, "Floor", StringComparison.OrdinalIgnoreCase))
                return Math.Floor(km);
            if (string.Equals(mode, "Round", StringComparison.OrdinalIgnoreCase))
                return Math.Round(km, 0, MidpointRounding.AwayFromZero);
            return Math.Ceiling(km);
        }
    }
}
