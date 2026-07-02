using RomanaWeb.Classes;

namespace RomanaWeb.Helper.Interface
{
    public class QuoteRequest
    {
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }
        public double DropoffLat { get; set; }
        public double DropoffLng { get; set; }
        /// <summary>Optional pre-computed distance. If supplied, lat/lng are ignored.</summary>
        public double? DistanceKm { get; set; }
        public int? CityId { get; set; }
        public int? RestaurantId { get; set; }
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
        /// <summary>city | zone | distance | minimum</summary>
        public string? PricingSource { get; set; }
        public string? FromZone { get; set; }
        public string? ToZone { get; set; }
        public bool MinChargeApplied { get; set; }
    }

    public interface IPricingService
    {
        Task<ResObj> Quote(QuoteRequest request);
    }
}
