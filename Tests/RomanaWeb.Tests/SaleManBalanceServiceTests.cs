using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Tests;

public class SaleManBalanceServiceTests
{
    [Fact]
    public async Task AccrueOnDelivered_AddsDualBalances_Idempotent()
    {
        var ctx = TestDbContext.New();
        ctx.SaleMan.Add(new SaleMan { SaleManId = 1, Name = "علي", Phone = "0770", IsActive = true });
        ctx.Orders.Add(new Orders
        {
            OrderId = 10,
            OrderNo = 100,
            SaleManId = 1,
            Total = 20000,
            NetAmount = 18000,
            CostDelivery = 3000,
            IsDelivered = true,
            IsDeliveryConfirmed = true,
            OrderDate = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();

        var svc = new SaleManBalanceService(ctx);
        var order = await ctx.Orders.FindAsync(10);
        await svc.AccrueOnDeliveredAsync(order!);
        await svc.AccrueOnDeliveredAsync(order!); // idempotent

        var bal = await ctx.SaleManBalance.FindAsync(1);
        Assert.NotNull(bal);
        Assert.Equal(18000m, bal!.FoodNetBalance);
        Assert.Equal(3000m, bal.DeliveryFeeBalance);
        Assert.Equal(1, ctx.SaleManBalanceEntry.Count());
    }

    [Fact]
    public async Task Settle_ZerosBalances_KeepsEntriesLinked()
    {
        var ctx = TestDbContext.New();
        ctx.SaleMan.Add(new SaleMan { SaleManId = 2, Name = "حسن", Phone = "0771", IsActive = true });
        ctx.Orders.Add(new Orders
        {
            OrderId = 20,
            OrderNo = 200,
            SaleManId = 2,
            Total = 10000,
            NetAmount = 10000,
            CostDelivery = 2500,
            IsDelivered = true,
            OrderDate = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();

        var svc = new SaleManBalanceService(ctx);
        await svc.AccrueOnDeliveredAsync((await ctx.Orders.FindAsync(20))!);

        var settle = await svc.SettleAsync(2, adminId: 99, note: "محاسبة");
        Assert.True(settle.success);

        var bal = await ctx.SaleManBalance.FindAsync(2);
        Assert.Equal(0m, bal!.FoodNetBalance);
        Assert.Equal(0m, bal.DeliveryFeeBalance);

        var entry = ctx.SaleManBalanceEntry.Single();
        Assert.NotNull(entry.SettlementId);

        var settlement = ctx.SaleManSettlement.Single();
        Assert.Equal(10000m, settlement.FoodNetAmount);
        Assert.Equal(2500m, settlement.DeliveryFeeAmount);
        Assert.Equal(99, settlement.SettledByAdminId);

        // driver still exists
        Assert.NotNull(await ctx.SaleMan.FindAsync(2));
        Assert.NotNull(await ctx.Orders.FindAsync(20));
    }

    [Fact]
    public void ResolveFoodNet_UsesNetAmountNotDelivery()
    {
        var order = new Orders { Total = 20000, NetAmount = 15000, CostDelivery = 4000 };
        Assert.Equal(15000m, SaleManBalanceService.ResolveFoodNet(order));
    }
}
