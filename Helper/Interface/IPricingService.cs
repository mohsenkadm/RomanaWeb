using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public class QuoteRequest
    {
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }
        public double DropoffLat { get; set; }
        public double DropoffLng { get; set; }
        /// <summary>Optional pre-computed route distance km (admin simulator / tests).</summary>
        public double? DistanceKm { get; set; }
        /// <summary>Optional driver approach point for zone entry (LZA).</summary>
        public double? DriverLat { get; set; }
        public double? DriverLng { get; set; }
        public int? CityId { get; set; }
        public int? RestaurantId { get; set; }
        /// <summary>When true, skip city-tier override (admin testing).</summary>
        public bool ForceZonePricing { get; set; }
    }

    public class QuoteResponse
    {
        public double DistanceKm { get; set; }
        public decimal PricePerKm { get; set; }
        public decimal DistanceFee { get; set; }
        public decimal ZoneFee { get; set; }
        public decimal MinimumCharge { get; set; }
        public decimal Total { get; set; }
        public decimal CityFee { get; set; }
        public decimal EcaFee { get; set; }
        public decimal LzaKm { get; set; }
        public decimal EcaKm { get; set; }
        public decimal EcaPricePerKm { get; set; }
        public double RouteDistanceKm { get; set; }
        /// <summary>city | zone_eca | zone | distance | minimum | near_restaurant</summary>
        public string? PricingSource { get; set; }
        public string? RouteSource { get; set; }
        public string? FromZone { get; set; }
        public string? ToZone { get; set; }
        public bool MinChargeApplied { get; set; }
        public bool NearRestaurantApplied { get; set; }
        public bool EcaCapApplied { get; set; }
        public bool MaxTotalCapApplied { get; set; }
        public decimal? MaxTotalDeliveryFee { get; set; }
    }

    public interface IPricingService
    {
        Task<ResObj> Quote(QuoteRequest request);
        Task<(bool covered, Zone? zone)> ResolveZoneAtPointAsync(double lat, double lng);
    }
}
