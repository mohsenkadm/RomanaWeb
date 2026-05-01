using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Tests;

public class PromoCodeServiceTests
{
    private static IConfiguration ConfigWith(bool firstUseForceGlobal = true)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PromoCodes:FirstUseForceGlobal"] = firstUseForceGlobal.ToString()
            })
            .Build();
    }

    private static PromoCodeService NewService(out RomanaWeb.Model.DB_Context ctx, bool firstUseForceGlobal = true)
    {
        ctx = TestDbContext.New();
        var mapper = new MapperConfiguration(_ => { }).CreateMapper();
        // _PromoCodeService dapper field is unused by the methods under test ⇒ pass null!.
        return new PromoCodeService(ctx, mapper, null!, ConfigWith(firstUseForceGlobal));
    }

    private static PromoCode SeedPromo(RomanaWeb.Model.DB_Context ctx, Action<PromoCode> tweak)
    {
        var p = new PromoCode
        {
            PromoName = "P",
            Percentage = 10,
            RestaurantId = 1,
            MaxOrders = 100,
            UsedOrders = 0,
            IsActive = true,
            IsForAllStores = true,
            DiscountType = "Percentage",
            MaxDiscountAmount = 0
        };
        tweak(p);
        ctx.PromoCodes.Add(p);
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear(); // avoid tracking conflicts when service re-attaches
        return p;
    }

    [Fact]
    public async Task Validate_RejectsInactivePromo()
    {
        var svc = NewService(out var ctx);
        SeedPromo(ctx, p => { p.PromoName = "X"; p.IsActive = false; });

        var res = await svc.ValidatePromoCode("X", restaurantId: 1);
        Assert.False(res.success);
    }

    [Fact]
    public async Task Validate_RejectsWhenUsageLimitReached()
    {
        var svc = NewService(out var ctx);
        SeedPromo(ctx, p => { p.PromoName = "USED"; p.MaxOrders = 5; p.UsedOrders = 5; });

        var res = await svc.ValidatePromoCode("USED", restaurantId: 1);
        Assert.False(res.success);
    }

    [Fact]
    public async Task Validate_StoreScoped_RejectsWrongStore()
    {
        var svc = NewService(out var ctx, firstUseForceGlobal: false);
        SeedPromo(ctx, p =>
        {
            p.PromoName = "STORE";
            p.IsForAllStores = false;
            p.RestaurantId = 7;
            p.UsedOrders = 1;        // not first use ⇒ flag does not relax check
            p.FirstUsedAt = DateTime.UtcNow;
        });

        var res = await svc.ValidatePromoCode("STORE", restaurantId: 99);
        Assert.False(res.success);
    }

    [Fact]
    public async Task Validate_FirstUseForceGlobal_AllowsCrossStoreOnFirstUse()
    {
        var svc = NewService(out var ctx, firstUseForceGlobal: true);
        SeedPromo(ctx, p =>
        {
            p.PromoName = "FIRST";
            p.IsForAllStores = false;
            p.RestaurantId = 7;
            p.UsedOrders = 0;
            p.FirstUsedAt = null; // never used ⇒ feature flag forces GLOBAL
        });

        var res = await svc.ValidatePromoCode("FIRST", restaurantId: 99);
        Assert.True(res.success);
    }

    [Fact]
    public async Task Apply_PercentageDiscount_CapsAtMaxDiscountAmount()
    {
        var svc = NewService(out var ctx);
        SeedPromo(ctx, p =>
        {
            p.PromoName = "PCT50";
            p.Percentage = 50;
            p.DiscountType = "Percentage";
            p.MaxDiscountAmount = 1000m; // hard ceiling
        });

        var res = await svc.ApplyPromoCode("PCT50", restaurantId: 1, orderTotal: 10_000m);
        Assert.True(res.success);

        // 50% of 10000 = 5000, but ceiling = 1000
        Assert.Equal(1000m, ObjectAccess.Get<decimal>(res.data!, "DiscountValue"));
        Assert.Equal(9000m, ObjectAccess.Get<decimal>(res.data!, "NetAmount"));
    }

    [Fact]
    public async Task Apply_FixedDiscount_NeverExceedsOrderTotal()
    {
        var svc = NewService(out var ctx);
        SeedPromo(ctx, p =>
        {
            p.PromoName = "FIX";
            p.DiscountType = "Fixed";
            p.DiscountAmount = 5000m;
        });

        var res = await svc.ApplyPromoCode("FIX", restaurantId: 1, orderTotal: 3000m);
        Assert.True(res.success);

        Assert.Equal(3000m, ObjectAccess.Get<decimal>(res.data!, "DiscountValue"));
        Assert.Equal(0m, ObjectAccess.Get<decimal>(res.data!, "NetAmount"));
    }

    [Fact]
    public async Task Apply_StoreScoped_FlagsFundedByStore()
    {
        var svc = NewService(out var ctx, firstUseForceGlobal: false);
        SeedPromo(ctx, p =>
        {
            p.PromoName = "STORE";
            p.IsForAllStores = false;
            p.RestaurantId = 1;
            p.DiscountType = "Fixed";
            p.DiscountAmount = 200m;
        });

        var res = await svc.ApplyPromoCode("STORE", restaurantId: 1, orderTotal: 10_000m);
        Assert.True(res.success);

        Assert.True(ObjectAccess.Get<bool>(res.data!, "FundedByStore"));
        Assert.False(ObjectAccess.Get<bool>(res.data!, "FundedByPlatform"));
    }

    [Fact]
    public async Task Apply_GlobalPromo_FlagsFundedByPlatform()
    {
        var svc = NewService(out var ctx);
        SeedPromo(ctx, p =>
        {
            p.PromoName = "GLOBAL";
            p.IsForAllStores = true;
            p.DiscountType = "Fixed";
            p.DiscountAmount = 200m;
        });

        var res = await svc.ApplyPromoCode("GLOBAL", restaurantId: 1, orderTotal: 10_000m);
        Assert.True(res.success);

        Assert.False(ObjectAccess.Get<bool>(res.data!, "FundedByStore"));
        Assert.True(ObjectAccess.Get<bool>(res.data!, "FundedByPlatform"));
    }

    [Fact]
    public async Task Apply_StampsFirstUsedAtAndIncrementsUsedOrders()
    {
        var svc = NewService(out var ctx);
        SeedPromo(ctx, p =>
        {
            p.PromoName = "ONCE";
            p.DiscountType = "Fixed";
            p.DiscountAmount = 100m;
            p.MaxOrders = 1;
        });

        var first = await svc.ApplyPromoCode("ONCE", 1, 1000m);
        Assert.True(first.success);

        ctx.ChangeTracker.Clear();
        var stored = ctx.PromoCodes.AsNoTracking().Single(p => p.PromoName == "ONCE");
        Assert.NotNull(stored.FirstUsedAt);
        Assert.Equal(1, stored.UsedOrders);
        // Auto-deactivated when MaxOrders reached.
        Assert.False(stored.IsActive);

        // Second attempt rejected (limit reached / inactive).
        var second = await svc.ApplyPromoCode("ONCE", 1, 1000m);
        Assert.False(second.success);
    }
}
