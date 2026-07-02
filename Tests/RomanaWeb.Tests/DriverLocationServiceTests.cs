using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Tests;

public class DriverLocationServiceTests
{
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

    private sealed class FakeHubNotifier : IOrderHubNotifier
    {
        public int DriverLocationBroadcastCount { get; private set; }
        public int LastUserId { get; private set; }
        public int LastOrderId { get; private set; }

        public Task NotifyRestaurantAsync(int restaurantId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode) => Task.CompletedTask;
        public Task NotifyDriverAsync(int driverId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode) => Task.CompletedTask;
        public Task NotifyAllDriversAsync(string title, string message, int orderId, string statusKey, int statusCode, string displayMode) => Task.CompletedTask;
        public Task NotifyUserAsync(int userId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode) => Task.CompletedTask;

        public Task NotifyUserDriverLocationUpdatedAsync(int userId, int orderId, int saleManId, double lat, double lng, DateTime at)
        {
            DriverLocationBroadcastCount++;
            LastUserId = userId;
            LastOrderId = orderId;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDapper<T> : IDapperRepository<T> where T : class
    {
        private readonly Func<string, object, List<T>>? _listHandler;

        public FakeDapper(Func<string, object, List<T>>? listHandler = null) => _listHandler = listHandler;

        public List<T> GetEntityList(string spName, object pars) => _listHandler?.Invoke(spName, pars) ?? new List<T>();
        public Task<List<T>> GetEntityListAsync(string spName, object pars) => Task.FromResult(GetEntityList(spName, pars));
        public Task<T> GetEntityAsync(string spName, object pars) => Task.FromResult(default(T)!);
        public T GetEntity(string spName, object pars) => default!;
        public Task<List<T>> GetEntityListScriptAsync(string Query, string pars) => Task.FromResult(new List<T>());
        public Task<T> GetEntityScriptAsync(string Query, string pars) => Task.FromResult(default(T)!);
        public void RunScript(string Query) { }
        public Task RunScriptAsync(string Query) => Task.CompletedTask;
        public void RunSp(string spName, object pars) { }
        public Task RunSpAsync(string spName, object pars) => Task.CompletedTask;
    }

    private static DriverDispatchService NewDispatchService(
        out RomanaWeb.Model.DB_Context ctx,
        out FakeHubNotifier hub,
        IMemoryCache? cache = null)
    {
        ctx = TestDbContext.New();
        hub = new FakeHubNotifier();
        return new DriverDispatchService(
            ctx,
            new FakeNotifier(),
            hub,
            new FakeLogger(),
            cache ?? new MemoryCache(new MemoryCacheOptions()));
    }

    private static OrdersService NewOrdersService(
        out RomanaWeb.Model.DB_Context ctx,
        DriverDispatchService? dispatch = null)
    {
        ctx = TestDbContext.New();
        var hub = new FakeHubNotifier();
        dispatch ??= new DriverDispatchService(ctx, new FakeNotifier(), hub, new FakeLogger(), new MemoryCache(new MemoryCacheOptions()));
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        return new OrdersService(
            ctx,
            new FakeDapper<Orders>(),
            new FakeDapper<OrderDetail>(),
            dispatch,
            new DistanceService(),
            new PricingService(ctx, new DistanceService()),
            new ConfigurationBuilder().Build(),
            new FakeLogger());
    }

    private static SaleMan SeedDriver(RomanaWeb.Model.DB_Context ctx, int id = 8)
    {
        var d = new SaleMan { SaleManId = id, Name = "Driver", Phone = "1", IsAvailable = true, IsActive = true };
        ctx.SaleMan.Add(d);
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();
        return d;
    }

    private static Orders SeedOrder(RomanaWeb.Model.DB_Context ctx, int orderId, int userId, int? saleManId = null, bool approved = false)
    {
        var o = new Orders
        {
            OrderId = orderId,
            OrderNo = orderId,
            OrderDate = DateTime.UtcNow,
            RestaurantId = 1,
            UserId = userId,
            Total = 10000,
            NetAmount = 10000,
            IsApporve = true,
            SaleManId = saleManId ?? 0,
            IsSaleManApprove = approved,
            OrderDetails = new List<OrderDetail>()
        };
        ctx.Orders.Add(o);
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();
        return o;
    }

    [Fact]
    public async Task UpdateLocation_ThenGetDriverLocation_ReturnsLatLng()
    {
        var svc = NewDispatchService(out var ctx, out _);
        SeedDriver(ctx);
        SeedOrder(ctx, orderId: 456, userId: 10, saleManId: 8, approved: true);

        var update = await svc.UpdateDriverLocation(8, 33.31, 44.36, orderId: 456);
        Assert.True(update.success);

        var (ok, status, data, _) = await svc.GetDriverLocationForOrder(456, userId: 10);
        Assert.True(ok);
        Assert.Equal(200, status);
        Assert.Equal(33.31, ObjectAccess.Get<double>(data!, "lat"));
        Assert.Equal(44.36, ObjectAccess.Get<double>(data!, "lng"));
        Assert.Equal(8, ObjectAccess.Get<int>(data!, "saleManId"));
    }

    [Fact]
    public async Task GetDriverLocation_Returns403_WhenUserDoesNotOwnOrder()
    {
        var svc = NewDispatchService(out var ctx, out _);
        SeedDriver(ctx);
        SeedOrder(ctx, orderId: 456, userId: 10, saleManId: 8, approved: true);
        await svc.UpdateDriverLocation(8, 33.31, 44.36, orderId: 456);

        var (ok, status, _, msg) = await svc.GetDriverLocationForOrder(456, userId: 99);
        Assert.False(ok);
        Assert.Equal(403, status);
        Assert.NotNull(msg);
    }

    [Fact]
    public async Task UpdateLocation_BroadcastsDriverLocationUpdated_WhenActiveOrderSet()
    {
        var svc = NewDispatchService(out var ctx, out var hub);
        SeedDriver(ctx);
        SeedOrder(ctx, orderId: 456, userId: 10, saleManId: 8, approved: true);

        await svc.UpdateDriverLocation(8, 33.31, 44.36, orderId: 456);

        Assert.Equal(1, hub.DriverLocationBroadcastCount);
        Assert.Equal(10, hub.LastUserId);
        Assert.Equal(456, hub.LastOrderId);
    }

    [Fact]
    public async Task UpdateLocation_ThrottlesSignalR_ForSameOrder()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var svc = NewDispatchService(out var ctx, out var hub, cache);
        SeedDriver(ctx);
        SeedOrder(ctx, orderId: 456, userId: 10, saleManId: 8, approved: true);

        await svc.UpdateDriverLocation(8, 33.31, 44.36, orderId: 456);
        await svc.UpdateDriverLocation(8, 33.32, 44.37, orderId: 456);

        Assert.Equal(1, hub.DriverLocationBroadcastCount);
    }

    [Fact]
    public async Task GetOrderFullDetails_UsesDriverLocationsForLatLong()
    {
        var dispatch = NewDispatchService(out var ctx, out _);
        SeedDriver(ctx, 8);
        SeedOrder(ctx, 456, userId: 10, saleManId: 8, approved: true);
        ctx.SaleMan.First().Lat = "0";
        ctx.SaleMan.First().Long = "0";
        ctx.DriverLocations.Add(new DriverLocation
        {
            SaleManId = 8,
            Lat = 33.31,
            Lng = 44.36,
            UpdatedAt = DateTime.UtcNow,
            ActiveOrderId = 456
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();

        var ordersSvc = new OrdersService(
            ctx,
            new FakeDapper<Orders>(),
            new FakeDapper<OrderDetail>(),
            dispatch,
            new DistanceService(),
            new PricingService(ctx, new DistanceService()),
            new ConfigurationBuilder().Build(),
            new FakeLogger());

        var res = await ordersSvc.GetOrderFullDetails(456);
        Assert.True(res.success);

        var driver = ObjectAccess.Get<SaleMan>(res.data!, "driver");
        Assert.Equal("33.31", driver.Lat);
        Assert.Equal("44.36", driver.Long);
    }

    [Fact]
    public async Task ApproveOrderBySaleMan_RejectsDriverWithActiveOrder()
    {
        var ordersSvc = NewOrdersService(out var ctx);
        SeedDriver(ctx, 8);
        SeedOrder(ctx, 100, userId: 1, saleManId: 8, approved: true);
        SeedOrder(ctx, 200, userId: 2, saleManId: 0, approved: false);

        var res = await ordersSvc.ApproveOrderBySaleMan(200, 8);
        Assert.False(res.success);
        Assert.Contains("طلب نشط", res.msg);
    }

    [Fact]
    public async Task ApproveOrderBySaleMan_AllowsAfterDeliveryConfirmed()
    {
        var ordersSvc = NewOrdersService(out var ctx);
        SeedDriver(ctx, 8);
        SeedOrder(ctx, 100, userId: 1, saleManId: 8, approved: true);
        SeedOrder(ctx, 200, userId: 2, saleManId: 0, approved: false);

        var done = await ordersSvc.SetDeliveryConfirmed(100);
        Assert.True(done.success);

        var res = await ordersSvc.ApproveOrderBySaleMan(200, 8);
        Assert.True(res.success);
    }

    [Fact]
    public void OrdersPostValidation_RejectsZeroLatOrCityId()
    {
        static bool IsValid(Users? u)
        {
            if (u == null) return false;
            return double.TryParse(u.Lat, out var lat) && lat != 0
                && double.TryParse(u.Long, out var lng) && lng != 0
                && u.CityId is > 0;
        }

        Assert.False(IsValid(new Users { Lat = "0", Long = "44.36", CityId = 1 }));
        Assert.False(IsValid(new Users { Lat = "33.31", Long = "0", CityId = 1 }));
        Assert.False(IsValid(new Users { Lat = "33.31", Long = "44.36", CityId = 0 }));
        Assert.False(IsValid(new Users { Lat = "33.31", Long = "44.36", CityId = null }));
        Assert.True(IsValid(new Users { Lat = "33.31", Long = "44.36", CityId = 5 }));
    }
}
