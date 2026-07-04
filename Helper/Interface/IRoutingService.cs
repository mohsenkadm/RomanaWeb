namespace RomanaWeb.Helper.Interface
{
    public interface IRoutingService
    {
        /// <summary>Road distance in km. Uses OSRM with Haversine×1.3 fallback.</summary>
        Task<RoutingResult> GetRouteDistanceKmAsync(double fromLat, double fromLng, double toLat, double toLng);
    }

    public class RoutingResult
    {
        public double DistanceKm { get; set; }
        /// <summary>osrm | haversine_fallback</summary>
        public string Source { get; set; } = "haversine_fallback";
        /// <summary>Route path as [lat, lng] pairs for map display.</summary>
        public List<RoutePoint>? Path { get; set; }
    }

    public class RoutePoint
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
