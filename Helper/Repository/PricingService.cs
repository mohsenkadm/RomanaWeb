using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    /// <summary>
    /// Unified pricing engine: near-restaurant → zone LZA/ECA → city → minimum → distance.
    /// PDF Rev.0001: FinalPrice = ZonePrice + min(ECA × EcaPricePerKm, MaxEcaFee).
    /// </summary>
    public class PricingService : IPricingService, IRegisterScopped
    {
        private readonly DB_Context _context;
        private readonly IDistanceService _distance;
        private readonly IRoutingService _routing;

        public PricingService(DB_Context context, IDistanceService distance, IRoutingService routing)
        {
            _context = context;
            _distance = distance;
            _routing = routing;
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
                               RoundingMode = "Ceil",
                               IqdRoundingStep = 250
                           };

            int iqdStep = settings.IqdRoundingStep > 0 ? settings.IqdRoundingStep : 250;

            if (request.DistanceKm.HasValue && request.DistanceKm.Value > 0
                && request.PickupLat == 0 && request.DropoffLat == 0)
            {
                return await QuoteFromDistanceOnlyAsync(request, settings, iqdStep);
            }

            if (!_distance.IsValidCoord(request.PickupLat, request.PickupLng) ||
                !_distance.IsValidCoord(request.DropoffLat, request.DropoffLng))
                return Result.Return(false, "يجب تحديد إحداثيات الاستلام والتسليم");

            double pickupToDropoffKm = _distance.RoundKm(_distance.HaversineKm(
                request.PickupLat, request.PickupLng, request.DropoffLat, request.DropoffLng));

            var zones = await _context.Zone.AsNoTracking().Where(z => z.IsActive).ToListAsync();
            var pickupZone = ResolveZoneEntity(zones, request.PickupLng, request.PickupLat);
            var dropoffZone = ResolveZoneEntity(zones, request.DropoffLng, request.DropoffLat);

            var nearResult = TryNearRestaurantPricing(pickupZone, dropoffZone, pickupToDropoffKm, iqdStep);
            if (nearResult != null)
                return Result.Return(true, nearResult);

            if (!request.ForceZonePricing)
            {
                var cityResult = await TryCityPricingAsync(request, settings, pickupToDropoffKm, iqdStep);
                if (cityResult != null)
                    return Result.Return(true, cityResult);
            }

            var zoneResult = await TryZoneEcaPricingAsync(request, settings, zones, pickupZone, dropoffZone, iqdStep);
            if (zoneResult != null)
                return Result.Return(true, zoneResult);

            if ((decimal)pickupToDropoffKm < settings.MinChargeKmThreshold)
            {
                return Result.Return(true, new QuoteResponse
                {
                    DistanceKm = pickupToDropoffKm,
                    PricePerKm = settings.PricePerKm,
                    MinimumCharge = settings.MinChargeAmount,
                    MinChargeApplied = true,
                    Total = RoundIqd(settings.MinChargeAmount, iqdStep),
                    PricingSource = "minimum"
                });
            }

            decimal billableKm = ApplyKmRounding((decimal)pickupToDropoffKm, settings.RoundingMode);
            decimal distanceFee = billableKm * settings.PricePerKm;
            return Result.Return(true, new QuoteResponse
            {
                DistanceKm = pickupToDropoffKm,
                RouteDistanceKm = pickupToDropoffKm,
                PricePerKm = settings.PricePerKm,
                DistanceFee = distanceFee,
                Total = RoundIqd(distanceFee, iqdStep),
                PricingSource = "distance"
            });
        }

        public async Task<(bool covered, Zone? zone)> ResolveZoneAtPointAsync(double lat, double lng)
        {
            var zones = await _context.Zone.AsNoTracking().Where(z => z.IsActive).ToListAsync();
            var zone = ResolveZoneEntity(zones, lng, lat);
            return (zone != null, zone);
        }

        private async Task<ResObj> QuoteFromDistanceOnlyAsync(QuoteRequest request, AppSettings settings, int iqdStep)
        {
            double distanceKm = request.DistanceKm!.Value;
            if (distanceKm <= 0)
                return Result.Return(false, "المسافة غير صالحة");

            if ((decimal)distanceKm < settings.MinChargeKmThreshold)
            {
                return Result.Return(true, new QuoteResponse
                {
                    DistanceKm = distanceKm,
                    RouteDistanceKm = distanceKm,
                    MinChargeApplied = true,
                    Total = RoundIqd(settings.MinChargeAmount, iqdStep),
                    PricingSource = "minimum"
                });
            }

            decimal billable = ApplyKmRounding((decimal)distanceKm, settings.RoundingMode);
            decimal fee = billable * settings.PricePerKm;
            return Result.Return(true, new QuoteResponse
            {
                DistanceKm = distanceKm,
                RouteDistanceKm = distanceKm,
                DistanceFee = fee,
                Total = RoundIqd(fee, iqdStep),
                PricingSource = "distance"
            });
        }

        private static QuoteResponse? TryNearRestaurantPricing(
            Zone? pickupZone, Zone? dropoffZone, double pickupToDropoffKm, int iqdStep)
        {
            var zone = dropoffZone ?? pickupZone;
            if (zone?.NearRestaurantPrice is not > 0) return null;

            double threshold = (double)zone.NearRestaurantKm;
            if (threshold <= 0) threshold = 1;

            if (pickupToDropoffKm >= threshold) return null;

            decimal total = RoundIqd(zone.NearRestaurantPrice.Value, iqdStep);
            bool maxTotalCapApplied = ApplyMaxTotalCap(zone, ref total, iqdStep);

            return new QuoteResponse
            {
                DistanceKm = pickupToDropoffKm,
                RouteDistanceKm = pickupToDropoffKm,
                ZoneFee = zone.NearRestaurantPrice.Value,
                Total = total,
                NearRestaurantApplied = true,
                MaxTotalCapApplied = maxTotalCapApplied,
                MaxTotalDeliveryFee = zone.MaxTotalDeliveryFee,
                PricingSource = "near_restaurant",
                FromZone = pickupZone?.Name,
                ToZone = dropoffZone?.Name ?? pickupZone?.Name
            };
        }

        private async Task<QuoteResponse?> TryCityPricingAsync(
            QuoteRequest request, AppSettings settings, double distanceKm, int iqdStep)
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
                RouteDistanceKm = distanceKm,
                CityFee = cityRow.CostDelivery.Value,
                Total = RoundIqd(cityRow.CostDelivery.Value, iqdStep),
                PricingSource = "city"
            };
        }

        private async Task<QuoteResponse?> TryZoneEcaPricingAsync(
            QuoteRequest request, AppSettings settings, List<Zone> zones,
            Zone? pickupZone, Zone? dropoffZone, int iqdStep)
        {
            if (pickupZone == null || dropoffZone == null)
                return null;

            var matrix = await _context.ZonePrice.AsNoTracking()
                .FirstOrDefaultAsync(p => p.FromZoneId == pickupZone.ZoneId && p.ToZoneId == dropoffZone.ZoneId);
            decimal zoneFee = matrix?.Price
                ?? dropoffZone.BaseDeliveryPrice
                ?? pickupZone.BaseDeliveryPrice
                ?? 0m;
            if (zoneFee <= 0) return null;

            double routeKm;
            string routeSource;
            if (request.DistanceKm.HasValue && request.DistanceKm.Value > 0)
            {
                routeKm = request.DistanceKm.Value;
                routeSource = "manual";
            }
            else
            {
                if (!ZoneGeometryHelper.TryParseRing(dropoffZone.GeoJson, out var ring))
                    return null;

                var (entryLng, entryLat) = ZoneGeometryHelper.FindEntryPointOnBoundary(
                    ring,
                    request.PickupLng, request.PickupLat,
                    request.DropoffLng, request.DropoffLat);

                var route = await _routing.GetRouteDistanceKmAsync(
                    entryLat, entryLng, request.DropoffLat, request.DropoffLng);
                routeKm = route.DistanceKm;
                routeSource = route.Source;
            }

            decimal lza = dropoffZone.LzaKm;
            decimal ecaKm = routeKm > (double)lza ? (decimal)(routeKm - (double)lza) : 0m;
            ecaKm = Math.Round(ecaKm, 2, MidpointRounding.AwayFromZero);

            decimal ecaRaw = ecaKm * dropoffZone.EcaPricePerKm;
            bool ecaCapApplied = false;
            decimal ecaFee = ecaRaw;
            if (dropoffZone.MaxEcaFee > 0 && ecaRaw > dropoffZone.MaxEcaFee)
            {
                ecaFee = dropoffZone.MaxEcaFee;
                ecaCapApplied = true;
            }

            decimal total = RoundIqd(zoneFee + ecaFee, iqdStep);
            bool maxTotalCapApplied = ApplyMaxTotalCap(dropoffZone, ref total, iqdStep);

            return new QuoteResponse
            {
                DistanceKm = _distance.RoundKm(_distance.HaversineKm(
                    request.PickupLat, request.PickupLng, request.DropoffLat, request.DropoffLng)),
                RouteDistanceKm = routeKm,
                RouteSource = routeSource,
                ZoneFee = zoneFee,
                EcaFee = ecaFee,
                EcaKm = ecaKm,
                LzaKm = lza,
                EcaPricePerKm = dropoffZone.EcaPricePerKm,
                Total = total,
                FromZone = pickupZone.Name,
                ToZone = dropoffZone.Name,
                PricingSource = ecaKm > 0 ? "zone_eca" : "zone",
                EcaCapApplied = ecaCapApplied,
                MaxTotalCapApplied = maxTotalCapApplied,
                MaxTotalDeliveryFee = dropoffZone.MaxTotalDeliveryFee
            };
        }

        private static bool ApplyMaxTotalCap(Zone zone, ref decimal total, int iqdStep)
        {
            if (zone.MaxTotalDeliveryFee is not > 0) return false;
            decimal cap = RoundIqd(zone.MaxTotalDeliveryFee.Value, iqdStep);
            if (total <= cap) return false;
            total = cap;
            return true;
        }

        private static Zone? ResolveZoneEntity(List<Zone> zones, double lng, double lat)
        {
            foreach (var z in zones)
            {
                if (!ZoneGeometryHelper.TryParseRing(z.GeoJson, out var ring)) continue;
                if (ZoneGeometryHelper.PointInPolygon(ring, lng, lat)) return z;
            }
            return null;
        }

        public static decimal RoundIqd(decimal amount, int step = 250)
        {
            if (step <= 0)
                return Math.Round(amount, 0, MidpointRounding.AwayFromZero);
            return Math.Round(amount / step, 0, MidpointRounding.AwayFromZero) * step;
        }

        private static decimal ApplyKmRounding(decimal km, string mode)
        {
            if (string.Equals(mode, "Floor", StringComparison.OrdinalIgnoreCase))
                return Math.Floor(km);
            if (string.Equals(mode, "Round", StringComparison.OrdinalIgnoreCase))
                return Math.Round(km, 0, MidpointRounding.AwayFromZero);
            return Math.Ceiling(km);
        }
    }
}
