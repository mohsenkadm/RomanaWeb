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
    }

    public interface IPricingService
    {
        // Section 2.3: returns the breakdown used by Customer app order summary screen.
        Task<ResObj> Quote(QuoteRequest request);
    }
}
