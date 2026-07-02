using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;
using RomanaWeb.Models.EntityMapper;

namespace RomanaWeb.Tests;

public class NearbyDriverOrdersTests
{
    private sealed class FakeDapper<T> : IDapperRepository<T> where T : class
    {
        public List<T> GetEntityList(string spName, object pars) => new();
        public Task<List<T>> GetEntityListAsync(string spName, object pars) => Task.FromResult(new List<T>());
        public Task<T> GetEntityAsync(string spName, object pars) => Task.FromResult(default(T)!);
        public T GetEntity(string spName, object pars) => default!;
        public Task<List<T>> GetEntityListScriptAsync(string Query, string pars) => Task.FromResult(new List<T>());
        public Task<T> GetEntityScriptAsync(string Query, string pars) => Task.FromResult(default(T)!);
        public void RunScript(string Query) { }
        public Task RunScriptAsync(string Query) => Task.CompletedTask;
        public void RunSp(string spName, object pars) { }
        public Task RunSpAsync(string spName, object pars) => Task.CompletedTask;
    }

    private sealed class FakeNotifier : INotificationDispatcher
    {
        public Task SendAsync(NotificationAudience audience, int recipientId, NotificationPayload payload) => Task.CompletedTask;
        public Task BroadcastAsync(NotificationAudience audience, NotificationPayload payload) => Task.CompletedTask;
        public Task SendManyAsync(NotificationAudience audience, IEnumerable<int> recipientIds, NotificationPayload payload) => Task.CompletedTask;
    }

    private sealed class FakeLogger : ILoggerRepository
    {
        public void Write(Exception exception, string message) { }
        public Task WriteAsync(Exception exception, string message) => Task.CompletedTask;
    }

    private sealed class FakeHub : IOrderHubNotifier
    {
        public Task NotifyRestaurantAsync(int restaurantId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode) => Task.CompletedTask;
        public Task NotifyUserAsync(int userId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode) => Task.CompletedTask;
        public Task NotifyDriverAsync(int driverId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode) => Task.CompletedTask;
        public Task NotifyAllDriversAsync(string title, string message, int orderId, string statusKey, int statusCode, string displayMode) => Task.CompletedTask;
        public Task NotifyUserDriverLocationUpdatedAsync(int userId, int orderId, int saleManId, double lat, double lng, DateTime at) => Task.CompletedTask;
    }

    private static OrdersService NewOrdersService(RomanaWeb.Model.DB_Context ctx)
    {
        var distance = new DistanceService();
        var pricing = new PricingService(ctx, distance);
        var dispatch = new DriverDispatchService(ctx, new FakeNotifier(), new FakeHub(), new FakeLogger(), new MemoryCache(new MemoryCacheOptions()));
        return new OrdersService(
            ctx,
            new FakeDapper<Orders>(),
            new FakeDapper<OrderDetail>(),
            dispatch,
            distance,
            pricing,
            new ConfigurationBuilder().Build(),
            new FakeLogger());
    }

    [Fact]
    public async Task GetNearbyDriverOrders_CalculatesDistances()
    {
        var ctx = TestDbContext.New();
        ctx.Restaurant.Add(new Restaurant
        {
            RestaurantId = 1,
            Name = "مطعم X",
            Phone = "1",
            Lat = "33.3000",
            Long = "44.3500"
        });
        ctx.Users.Add(new Users { UserId = 5, Name = "Ali", Phone = "07", Lat = "33.3200", Long = "44.3800" });
        ctx.Orders.Add(new Orders
        {
            OrderId = 456,
            OrderNo = 1001,
            OrderDate = DateTime.UtcNow,
            RestaurantId = 1,
            UserId = 5,
            Total = 10000,
            NetAmount = 10000,
            IsApporve = true,
            Lat = "33.3200",
            Long = "44.3800",
            CostDelivery = 3500
        });
        ctx.AppSettings.Add(new AppSettings
        {
            PricePerKm = 500,
            MinChargeKmThreshold = 0.5m,
            MinChargeAmount = 500m,
            RoundingMode = "Ceil"
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = NewOrdersService(ctx);
        var res = await svc.GetNearbyDriverOrders(8, 33.3100, 44.3600, radiusKm: 50);

        Assert.True(res.success);
        var list = Assert.IsType<List<NearbyDriverOrderDto>>(res.data);
        Assert.Single(list);
        var item = list[0];
        Assert.True(item.DistanceToPickupKm > 0);
        Assert.True(item.PickupToDropoffKm > 0);
        Assert.True(item.EstimatedFee > 0);
    }

    [Fact]
    public async Task GetNearbyDriverOrders_SkipsOrdersWithZeroCoords()
    {
        var ctx = TestDbContext.New();
        ctx.Restaurant.Add(new Restaurant { RestaurantId = 1, Name = "R", Phone = "1", Lat = "0", Long = "0" });
        ctx.Users.Add(new Users { UserId = 5, Name = "Ali", Phone = "07", Lat = "0", Long = "0" });
        ctx.Orders.Add(new Orders
        {
            OrderId = 1,
            OrderNo = 1,
            OrderDate = DateTime.UtcNow,
            RestaurantId = 1,
            UserId = 5,
            Total = 1000,
            NetAmount = 1000,
            IsApporve = true
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var svc = NewOrdersService(ctx);
        var res = await svc.GetNearbyDriverOrders(8, 33.31, 44.36, 50);

        Assert.True(res.success);
        var list = Assert.IsType<List<NearbyDriverOrderDto>>(res.data);
        Assert.Empty(list);
    }
}
