using RomanaWeb.Models.Entity;

namespace RomanaWeb.Tests;

internal static class ZoneTestSeeder
{
    public const string TestZoneGeoJson =
        "{\"type\":\"Polygon\",\"coordinates\":[[[44.30,33.30],[44.42,33.30],[44.42,33.34],[44.30,33.34],[44.30,33.30]]]}";

    public const double InZoneLat = 33.32;
    public const double InZoneLng = 44.38;

    public static int SeedZone(RomanaWeb.Model.DB_Context ctx, string name = "TestZone")
    {
        ctx.Zone.Add(new Zone
        {
            Name = name,
            GeoJson = TestZoneGeoJson,
            IsActive = true,
            LzaKm = 3,
            EcaPricePerKm = 250
        });
        ctx.SaveChanges();
        return ctx.Zone.Single(z => z.Name == name).ZoneId;
    }

    public static void AssignDriverToZone(RomanaWeb.Model.DB_Context ctx, int saleManId, int zoneId)
    {
        ctx.SaleManZone.Add(new SaleManZone { SaleManId = saleManId, ZoneId = zoneId });
        ctx.SaveChanges();
    }

    public static void AssignRestaurantToZone(RomanaWeb.Model.DB_Context ctx, int restaurantId, int zoneId)
    {
        ctx.RestaurantZone.Add(new RestaurantZone { RestaurantId = restaurantId, ZoneId = zoneId });
        ctx.SaveChanges();
    }

    public static SaleMan SeedAvailableDriver(RomanaWeb.Model.DB_Context ctx, int saleManId, int zoneId)
    {
        ctx.SaleMan.Add(new SaleMan
        {
            SaleManId = saleManId,
            Name = "Driver",
            Phone = "07",
            IsAvailable = true,
            IsActive = true
        });
        ctx.SaveChanges();
        AssignDriverToZone(ctx, saleManId, zoneId);
        ctx.ChangeTracker.Clear();
        return ctx.SaleMan.Single(d => d.SaleManId == saleManId);
    }
}
