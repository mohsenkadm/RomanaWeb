using RomanaWeb.Helper.Repository;

namespace RomanaWeb.Tests;

public class DistanceServiceTests
{
    private readonly DistanceService _svc = new();

    [Fact]
    public void HaversineKm_KnownPoints_ReturnsPositiveDistance()
    {
        // Driver ~ restaurant ~ customer in Baghdad area
        double driverToPickup = _svc.HaversineKm(33.31, 44.36, 33.30, 44.35);
        double pickupToDropoff = _svc.HaversineKm(33.30, 44.35, 33.32, 44.38);

        Assert.True(driverToPickup > 0);
        Assert.True(pickupToDropoff > 0);
        Assert.Equal(Math.Round(driverToPickup, 2), _svc.RoundKm(driverToPickup));
    }

    [Fact]
    public void IsValidCoord_RejectsZero()
    {
        Assert.False(_svc.IsValidCoord(0, 44.36));
        Assert.False(_svc.IsValidCoord(33.31, 0));
        Assert.True(_svc.IsValidCoord(33.31, 44.36));
    }

    [Fact]
    public void TryParseCoord_ParsesInvariantStrings()
    {
        Assert.True(_svc.TryParseCoord("33.3150", "44.3660", out var lat, out var lng));
        Assert.Equal(33.3150, lat);
        Assert.Equal(44.3660, lng);
    }
}
