using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Tests;

public class PricingServiceTests
{
    private static PricingService NewService(out RomanaWeb.Model.DB_Context ctx, AppSettings? settings = null)
    {
        ctx = TestDbContext.New();
        if (settings != null)
        {
            ctx.AppSettings.Add(settings);
            ctx.SaveChanges();
            ctx.ChangeTracker.Clear();
        }
        return new PricingService(ctx, new DistanceService());
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
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.True(quote.MinChargeApplied);
        Assert.Equal(500m, quote.Total);
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

        var res = await svc.Quote(new QuoteRequest { DistanceKm = 4.2 });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.False(quote.MinChargeApplied);
        Assert.Equal(2500m, quote.Total);
        Assert.Equal(2500m, quote.DistanceFee);
        Assert.Equal(0m, quote.ZoneFee);
        Assert.Equal(4.2, quote.DistanceKm);
    }

    [Fact]
    public async Task Quote_FromCoords_CalculatesDistanceKm()
    {
        var svc = NewService(out _, new AppSettings
        {
            PricePerKm = 500,
            MinChargeKmThreshold = 0.5m,
            MinChargeAmount = 500m,
            RoundingMode = "Ceil"
        });

        var res = await svc.Quote(new QuoteRequest
        {
            PickupLat = 33.30,
            PickupLng = 44.35,
            DropoffLat = 33.32,
            DropoffLng = 44.38
        });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.True(quote.DistanceKm > 0);
    }

    [Fact]
    public async Task Quote_RejectsZeroCoords()
    {
        var svc = NewService(out _);
        var res = await svc.Quote(new QuoteRequest { PickupLat = 0, PickupLng = 44.35, DropoffLat = 33.32, DropoffLng = 44.38 });
        Assert.False(res.success);
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

        var res = await svc.Quote(new QuoteRequest { DistanceKm = 4.9 });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(2000m, quote.Total);
    }

    [Fact]
    public async Task Quote_NoSettingsRow_UsesSafeDefaults()
    {
        var svc = NewService(out _);
        var res = await svc.Quote(new QuoteRequest { DistanceKm = 3.0 });
        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(1500m, quote.Total);
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

        var svc = new PricingService(ctx, new DistanceService());
        var res = await svc.Quote(new QuoteRequest
        {
            PickupLng = 0.5, PickupLat = 0.5,
            DropoffLng = 2.5, DropoffLat = 0.5
        });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(4000m, quote.ZoneFee);
        Assert.Equal(1500m, quote.DistanceFee);
        Assert.Equal(5500m, quote.Total);
        Assert.Equal("A", quote.FromZone);
        Assert.Equal("B", quote.ToZone);
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

        var svc = new PricingService(ctx, new DistanceService());
        var res = await svc.Quote(new QuoteRequest
        {
            PickupLng = 1, PickupLat = 1,
            DropoffLng = 2, DropoffLat = 1,
            DistanceKm = 4
        });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(0m, quote.ZoneFee);
        Assert.Equal(2000m, quote.Total);
    }

    [Fact]
    public async Task Quote_RestaurantCityFee_TakesPriority()
    {
        var ctx = TestDbContext.New();
        ctx.AppSettings.Add(new AppSettings
        {
            PricePerKm = 500,
            MinChargeKmThreshold = 1.5m,
            MinChargeAmount = 500m,
            RoundingMode = "Ceil"
        });
        ctx.RestaurantCity.Add(new RestaurantCity
        {
            RestaurantId = 10,
            CityId = 5,
            CostDelivery = 3000m
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = new PricingService(ctx, new DistanceService());
        var res = await svc.Quote(new QuoteRequest
        {
            RestaurantId = 10,
            CityId = 5,
            DistanceKm = 3
        });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(3000m, quote.Total);
        Assert.Equal(3000m, quote.CityFee);
        Assert.Equal("city", quote.PricingSource);
    }
}
