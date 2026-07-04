using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;

namespace RomanaWeb.Tests;

internal sealed class FakeRoutingService : IRoutingService
{
    private readonly IDistanceService _distance = new DistanceService();

    public Task<RoutingResult> GetRouteDistanceKmAsync(double fromLat, double fromLng, double toLat, double toLng)
    {
        double km = _distance.RoundKm(_distance.HaversineKm(fromLat, fromLng, toLat, toLng) * 1.3);
        return Task.FromResult(new RoutingResult
        {
            DistanceKm = km,
            Source = "test",
            Path = new List<RoutePoint>
            {
                new() { Lat = fromLat, Lng = fromLng },
                new() { Lat = toLat, Lng = toLng }
            }
        });
    }
}
