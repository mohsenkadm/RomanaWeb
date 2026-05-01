using Microsoft.Extensions.Configuration;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Tests;

public class PricingServiceTests
{
    private static IConfiguration EmptyConfig() =>
        new ConfigurationBuilder().AddInMemoryCollection().Build();

    private static PricingService NewService(out RomanaWeb.Model.DB_Context ctx, AppSettings? settings = null)
    {
        ctx = TestDbContext.New();
        if (settings != null)
        {
            ctx.AppSettings.Add(settings);
            ctx.SaveChanges();
            ctx.ChangeTracker.Clear();
        }
        return new PricingService(ctx);
    }

    [Fact]
    public async Task Quote_BelowMinThreshold_AppliesMinCharge()
    {
        var svc = NewService(out _, new AppSettings
        {
            PricePerKm = 500,
            DefaultOrderCost = 3000,
            MinChargeKmThreshold = 1.5m,
            MinChargeAmount = 500m,
            RoundingMode = "Ceil"
        });

        var res = await svc.Quote(new QuoteRequest { DistanceKm = 0.8 });

        Assert.True(res.success);
        Assert.True(ObjectAccess.Get<bool>(res.data!, "min_charge_applied"));
        Assert.Equal(500m, ObjectAccess.Get<decimal>(res.data!, "total"));
    }

    [Fact]
    public async Task Quote_AboveThreshold_UsesCeilRounding()
    {
        var svc = NewService(out _, new AppSettings
        {
            PricePerKm = 500,
            MinChargeKmThreshold = 1.5m,
            MinChargeAmount = 500m,
            RoundingMode = "Ceil"
        });

        // 4.2 km ⇒ ceil ⇒ 5 km * 500 = 2500
        var res = await svc.Quote(new QuoteRequest { DistanceKm = 4.2 });

        Assert.True(res.success);
        Assert.False(ObjectAccess.Get<bool>(res.data!, "min_charge_applied"));
        Assert.Equal(2500m, ObjectAccess.Get<decimal>(res.data!, "total"));
        Assert.Equal(2500m, ObjectAccess.Get<decimal>(res.data!, "per_km_total"));
        Assert.Equal(0m, ObjectAccess.Get<decimal>(res.data!, "zone_fee"));
    }

    [Fact]
    public async Task Quote_FloorRounding_RoundsDown()
    {
        var svc = NewService(out _, new AppSettings
        {
            PricePerKm = 500,
            MinChargeKmThreshold = 1.5m,
            MinChargeAmount = 500m,
            RoundingMode = "Floor"
        });

        // 4.9 km ⇒ floor ⇒ 4 km * 500 = 2000
        var res = await svc.Quote(new QuoteRequest { DistanceKm = 4.9 });

        Assert.True(res.success);
        Assert.Equal(2000m, ObjectAccess.Get<decimal>(res.data!, "total"));
    }

    [Fact]
    public async Task Quote_NoSettingsRow_UsesSafeDefaults()
    {
        var svc = NewService(out _);
        var res = await svc.Quote(new QuoteRequest { DistanceKm = 3.0 });
        Assert.True(res.success);
        // Defaults: PricePerKm=500, Ceil ⇒ 3 * 500 = 1500
        Assert.Equal(1500m, ObjectAccess.Get<decimal>(res.data!, "total"));
    }

    [Fact]
    public async Task Quote_CrossZone_AddsZoneFeeAndCapsKm()
    {
        var ctx = TestDbContext.New();
        ctx.AppSettings.Add(new AppSettings
        {
            PricePerKm = 500,
            MinChargeKmThreshold = 1.5m,
            MinChargeAmount = 500m,
            RoundingMode = "Ceil",
            ZoneMaxKm = 3m
        });
        ctx.Zone.Add(new Zone
        {
            Name = "A",
            IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[1,0],[1,1],[0,1],[0,0]]]}"
        });
        ctx.Zone.Add(new Zone
        {
            Name = "B",
            IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[2,0],[3,0],[3,1],[2,1],[2,0]]]}"
        });
        ctx.SaveChanges();
        var aId = ctx.Zone.Single(z => z.Name == "A").ZoneId;
        var bId = ctx.Zone.Single(z => z.Name == "B").ZoneId;
        ctx.ZonePrice.Add(new ZonePrice { FromZoneId = aId, ToZoneId = bId, Price = 4000m });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = new PricingService(ctx);
        var res = await svc.Quote(new QuoteRequest
        {
            PickupLng = 0.5, PickupLat = 0.5,
            DropoffLng = 2.5, DropoffLat = 0.5
        });

        Assert.True(res.success);
        Assert.Equal(4000m, ObjectAccess.Get<decimal>(res.data!, "zone_fee"));
        Assert.Equal(1500m, ObjectAccess.Get<decimal>(res.data!, "per_km_total")); // 3 km cap * 500
        Assert.Equal(5500m, ObjectAccess.Get<decimal>(res.data!, "total"));
    }

    [Fact]
    public async Task Quote_SameZone_FallsBackToPerKm()
    {
        var ctx = TestDbContext.New();
        ctx.AppSettings.Add(new AppSettings
        {
            PricePerKm = 500,
            MinChargeKmThreshold = 1.5m,
            MinChargeAmount = 500m,
            RoundingMode = "Ceil"
        });
        ctx.Zone.Add(new Zone
        {
            Name = "Single",
            IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}"
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = new PricingService(ctx);
        var res = await svc.Quote(new QuoteRequest
        {
            PickupLng = 1, PickupLat = 1,
            DropoffLng = 2, DropoffLat = 1,
            DistanceKm = 4
        });

        Assert.True(res.success);
        Assert.Equal(0m, ObjectAccess.Get<decimal>(res.data!, "zone_fee"));
        Assert.Equal(2000m, ObjectAccess.Get<decimal>(res.data!, "total")); // 4 * 500
    }
}
