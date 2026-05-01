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
    /// Implements 2.1 (per-km + min charge + rounding) and 2.2 (zone-to-zone matrix
    /// with min/max km cap inside the destination zone).
    /// </summary>
    public class PricingService : IPricingService, IRegisterScopped
    {
        private readonly DB_Context _context;

        public PricingService(DB_Context context)
        {
            _context = context;
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

            double distanceKm = request.DistanceKm
                                ?? Haversine(request.PickupLat, request.PickupLng,
                                             request.DropoffLat, request.DropoffLng);

            // Section 2.1: minimum-charge rule.
            if ((decimal)distanceKm < settings.MinChargeKmThreshold)
            {
                var min = new
                {
                    distance_km = Math.Round(distanceKm, 2),
                    @base = 0m,
                    per_km_total = 0m,
                    zone_fee = 0m,
                    min_charge_applied = true,
                    total = settings.MinChargeAmount
                };
                return Result.Return(true, min);
            }

            // Section 2.2: try zone-based pricing first.
            var zoneResult = await TryZonePricing(request, settings, distanceKm);
            if (zoneResult != null) return Result.Return(true, zoneResult);

            // Section 2.1: per-km pricing fallback (same zone or no zones configured).
            decimal billableKm = ApplyRounding((decimal)distanceKm, settings.RoundingMode);
            decimal perKmTotal = billableKm * settings.PricePerKm;
            decimal total = perKmTotal;

            var result = new
            {
                distance_km = Math.Round(distanceKm, 2),
                @base = 0m,
                per_km_total = perKmTotal,
                zone_fee = 0m,
                min_charge_applied = false,
                total = total
            };
            return Result.Return(true, result);
        }

        // Section 2.2: zone detection + matrix lookup.
        // Returns null when zones are not configured or pickup/dropoff don't fall in any zone
        // (caller falls back to per-km pricing).
        private async Task<object?> TryZonePricing(QuoteRequest request, AppSettings settings, double distanceKm)
        {
            var zones = await _context.Zone.AsNoTracking().Where(z => z.IsActive).ToListAsync();
            if (zones.Count == 0) return null;

            int? pickupZone = ResolveZone(zones, request.PickupLng, request.PickupLat);
            int? dropoffZone = ResolveZone(zones, request.DropoffLng, request.DropoffLat);
            if (pickupZone == null || dropoffZone == null) return null;

            // Same zone => per-km pricing (2.2.2).
            if (pickupZone.Value == dropoffZone.Value) return null;

            // Cross-zone => base zone fee + per-km on distance inside the destination zone,
            // capped by AppSettings.ZoneMinKm/ZoneMaxKm (2.2.4).
            var entry = await _context.ZonePrice.AsNoTracking()
                .FirstOrDefaultAsync(p => p.FromZoneId == pickupZone.Value && p.ToZoneId == dropoffZone.Value);
            decimal zoneFee = entry?.Price ?? 0m;

            decimal billable = ApplyRounding((decimal)distanceKm, settings.RoundingMode);
            if (settings.ZoneMinKm > 0 && billable < settings.ZoneMinKm) billable = settings.ZoneMinKm;
            if (settings.ZoneMaxKm > 0 && billable > settings.ZoneMaxKm) billable = settings.ZoneMaxKm;
            decimal perKm = billable * settings.PricePerKm;

            return new
            {
                distance_km = Math.Round(distanceKm, 2),
                @base = zoneFee,
                per_km_total = perKm,
                zone_fee = zoneFee,
                min_charge_applied = false,
                from_zone_id = pickupZone.Value,
                to_zone_id = dropoffZone.Value,
                total = zoneFee + perKm
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
                catch { /* ignore malformed geo, treat as no-match */ }
            }
            return null;
        }

        // Ray-casting point-in-polygon. polygon is [(lng, lat), ...].
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
            // Default: Ceil
            return Math.Ceiling(km);
        }

        private static double Haversine(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371d;
            double dLat = (lat2 - lat1) * Math.PI / 180d;
            double dLng = (lng2 - lng1) * Math.PI / 180d;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180d) * Math.Cos(lat2 * Math.PI / 180d) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
