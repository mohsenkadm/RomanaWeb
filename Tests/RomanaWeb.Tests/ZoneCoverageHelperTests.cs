using RomanaWeb.Helper;

namespace RomanaWeb.Tests;

public class ZoneCoverageHelperTests
{
    [Fact]
    public void ServesZone_requires_assigned_zones_and_match()
    {
        Assert.False(ZoneCoverageHelper.ServesZone(null, 1));
        Assert.False(ZoneCoverageHelper.ServesZone(Array.Empty<int>(), 1));
        Assert.False(ZoneCoverageHelper.ServesZone(new[] { 1, 2 }, null));
        Assert.False(ZoneCoverageHelper.ServesZone(new[] { 1, 2 }, 3));
        Assert.True(ZoneCoverageHelper.ServesZone(new[] { 1, 2, 3 }, 2));
    }
}
