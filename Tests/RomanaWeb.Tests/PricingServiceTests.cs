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
        return new PricingService(ctx, new DistanceService(), new FakeRoutingService());
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
            RoundingMode = "Ceil",
            IqdRoundingStep = 250
        });

        var res = await svc.Quote(new QuoteRequest { DistanceKm = 0.8, PickupLat = 33.3, PickupLng = 44.3, DropoffLat = 33.31, DropoffLng = 44.31 });

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
            RoundingMode = "Ceil",
            IqdRoundingStep = 250
        });

        var res = await svc.Quote(new QuoteRequest { DistanceKm = 4.2, PickupLat = 0, DropoffLat = 0 });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.False(quote.MinChargeApplied);
        Assert.Equal(2500m, quote.Total);
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
    public async Task Quote_PdfExample_4km_Returns3250()
    {
        var ctx = TestDbContext.New();
        ctx.AppSettings.Add(new AppSettings { IqdRoundingStep = 250 });
        var from = new Zone
        {
            Name = "قضاء المدينة",
            IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[47.75,30.48],[47.82,30.48],[47.82,30.52],[47.75,30.52],[47.75,30.48]]]}",
            LzaKm = 3, EcaPricePerKm = 250, MaxEcaFee = 2500
        };
        var to = new Zone
        {
            Name = "الامام الصادق",
            IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[47.83,30.48],[47.90,30.48],[47.90,30.52],[47.83,30.52],[47.83,30.48]]]}",
            LzaKm = 3, EcaPricePerKm = 250, MaxEcaFee = 2500
        };
        ctx.Zone.AddRange(from, to);
        ctx.SaveChanges();
        var fromId = ctx.Zone.Single(z => z.Name == from.Name).ZoneId;
        var toId = ctx.Zone.Single(z => z.Name == to.Name).ZoneId;
        ctx.ZonePrice.Add(new ZonePrice { FromZoneId = fromId, ToZoneId = toId, Price = 3000m });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = new PricingService(ctx, new DistanceService(), new FakeRoutingService());
        var res = await svc.Quote(new QuoteRequest
        {
            ForceZonePricing = true,
            PickupLng = 47.78, PickupLat = 30.50,
            DropoffLng = 47.86, DropoffLat = 30.50,
            DistanceKm = 4.0
        });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(3000m, quote.ZoneFee);
        Assert.Equal(250m, quote.EcaFee);
        Assert.Equal(3250m, quote.Total);
        Assert.Equal("zone_eca", quote.PricingSource);
    }

    [Fact]
    public async Task Quote_PdfExample_5_2km_RoundsTo3500()
    {
        var ctx = TestDbContext.New();
        ctx.AppSettings.Add(new AppSettings { IqdRoundingStep = 250 });
        var from = new Zone
        {
            Name = "A",
            IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[1,0],[1,1],[0,1],[0,0]]]}",
            LzaKm = 3, EcaPricePerKm = 250, MaxEcaFee = 2500
        };
        var to = new Zone
        {
            Name = "B",
            IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[2,0],[3,0],[3,1],[2,1],[2,0]]]}",
            LzaKm = 3, EcaPricePerKm = 250, MaxEcaFee = 2500
        };
        ctx.Zone.AddRange(from, to);
        ctx.SaveChanges();
        ctx.ZonePrice.Add(new ZonePrice
        {
            FromZoneId = ctx.Zone.Single(z => z.Name == "A").ZoneId,
            ToZoneId = ctx.Zone.Single(z => z.Name == "B").ZoneId,
            Price = 3000m
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = new PricingService(ctx, new DistanceService(), new FakeRoutingService());
        var res = await svc.Quote(new QuoteRequest
        {
            ForceZonePricing = true,
            PickupLng = 0.5, PickupLat = 0.5,
            DropoffLng = 2.5, DropoffLat = 0.5,
            DistanceKm = 5.2
        });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(3500m, quote.Total);
    }

    [Fact]
    public async Task Quote_EcaFee_CappedByMaxEcaFee()
    {
        var ctx = TestDbContext.New();
        ctx.AppSettings.Add(new AppSettings { IqdRoundingStep = 250 });
        var from = new Zone
        {
            Name = "A", IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[1,0],[1,1],[0,1],[0,0]]]}",
            LzaKm = 3, EcaPricePerKm = 250, MaxEcaFee = 2500
        };
        var to = new Zone
        {
            Name = "B", IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[2,0],[3,0],[3,1],[2,1],[2,0]]]}",
            LzaKm = 3, EcaPricePerKm = 250, MaxEcaFee = 2500
        };
        ctx.Zone.AddRange(from, to);
        ctx.SaveChanges();
        ctx.ZonePrice.Add(new ZonePrice
        {
            FromZoneId = ctx.Zone.Single(z => z.Name == "A").ZoneId,
            ToZoneId = ctx.Zone.Single(z => z.Name == "B").ZoneId,
            Price = 3000m
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = new PricingService(ctx, new DistanceService(), new FakeRoutingService());
        var res = await svc.Quote(new QuoteRequest
        {
            ForceZonePricing = true,
            PickupLng = 0.5, PickupLat = 0.5,
            DropoffLng = 2.5, DropoffLat = 0.5,
            DistanceKm = 20.0
        });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(2500m, quote.EcaFee);
        Assert.True(quote.EcaCapApplied);
        Assert.Equal(5500m, quote.Total);
    }

    [Fact]
    public async Task Quote_Total_CappedByMaxTotalDeliveryFee()
    {
        var ctx = TestDbContext.New();
        ctx.AppSettings.Add(new AppSettings { IqdRoundingStep = 250 });
        var from = new Zone
        {
            Name = "A", IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[1,0],[1,1],[0,1],[0,0]]]}",
            LzaKm = 3, EcaPricePerKm = 250, MaxEcaFee = 2500, MaxTotalDeliveryFee = 4000m
        };
        var to = new Zone
        {
            Name = "B", IsActive = true,
            GeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[2,0],[3,0],[3,1],[2,1],[2,0]]]}",
            LzaKm = 3, EcaPricePerKm = 250, MaxEcaFee = 2500, MaxTotalDeliveryFee = 4000m
        };
        ctx.Zone.AddRange(from, to);
        ctx.SaveChanges();
        ctx.ZonePrice.Add(new ZonePrice
        {
            FromZoneId = ctx.Zone.Single(z => z.Name == "A").ZoneId,
            ToZoneId = ctx.Zone.Single(z => z.Name == "B").ZoneId,
            Price = 3000m
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = new PricingService(ctx, new DistanceService(), new FakeRoutingService());
        var res = await svc.Quote(new QuoteRequest
        {
            ForceZonePricing = true,
            PickupLng = 0.5, PickupLat = 0.5,
            DropoffLng = 2.5, DropoffLat = 0.5,
            DistanceKm = 20.0
        });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(4000m, quote.Total);
        Assert.True(quote.MaxTotalCapApplied);
    }

    [Fact]
    public async Task Quote_RoundIqd_Steps250()
    {
        Assert.Equal(3500m, PricingService.RoundIqd(3550m, 250));
        Assert.Equal(3250m, PricingService.RoundIqd(3250m, 250));
    }

    [Fact]
    public async Task Quote_RestaurantCityFee_TakesPriority()
    {
        var ctx = TestDbContext.New();
        ctx.AppSettings.Add(new AppSettings { IqdRoundingStep = 250 });
        ctx.RestaurantCity.Add(new RestaurantCity
        {
            RestaurantId = 10,
            CityId = 5,
            CostDelivery = 3000m
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = new PricingService(ctx, new DistanceService(), new FakeRoutingService());
        var res = await svc.Quote(new QuoteRequest
        {
            RestaurantId = 10,
            CityId = 5,
            PickupLat = 33.3, PickupLng = 44.3,
            DropoffLat = 33.31, DropoffLng = 44.31,
            DistanceKm = 3
        });

        Assert.True(res.success);
        var quote = Assert.IsType<QuoteResponse>(res.data);
        Assert.Equal(3000m, quote.Total);
        Assert.Equal("city", quote.PricingSource);
    }
}
