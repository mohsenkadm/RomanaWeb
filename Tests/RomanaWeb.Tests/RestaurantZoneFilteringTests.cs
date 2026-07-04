using RomanaWeb.Helper.Repository;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Tests;

public class RestaurantZoneFilteringTests
{
    private sealed class FakeDapper<T> : Helper.Interface.IDapperRepository<T> where T : class
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

    private static RestaurantService NewRestaurantService(RomanaWeb.Model.DB_Context ctx)
    {
        var distance = new DistanceService();
        return new RestaurantService(ctx, new FakeDapper<Restaurant>(), distance,
            new PricingService(ctx, distance, new FakeRoutingService()));
    }

    private static Restaurant SeedRestaurant(RomanaWeb.Model.DB_Context ctx, int id, string name)
    {
        var r = new Restaurant
        {
            RestaurantId = id,
            Name = name,
            Phone = "1",
            Lat = ZoneTestSeeder.InZoneLat.ToString("F4"),
            Long = ZoneTestSeeder.InZoneLng.ToString("F4"),
            IsActive = true,
            IsApproved = true,
            IsDelete = false
        };
        ctx.Restaurant.Add(r);
        ctx.SaveChanges();
        return r;
    }

    [Fact]
    public async Task GetByUserLocation_HidesRestaurant_WhenCustomerZoneNotServed()
    {
        var ctx = TestDbContext.New();
        var zoneA = ZoneTestSeeder.SeedZone(ctx, "ZoneA");
        var zoneB = ZoneTestSeeder.SeedZone(ctx, "ZoneB");
        ZoneTestSeeder.SeedAvailableDriver(ctx, 8, zoneA);

        var restaurant = SeedRestaurant(ctx, 1, "مطعم A+B");
        ZoneTestSeeder.AssignRestaurantToZone(ctx, restaurant.RestaurantId, zoneA);
        ZoneTestSeeder.AssignRestaurantToZone(ctx, restaurant.RestaurantId, zoneB);
        ctx.ChangeTracker.Clear();

        var svc = NewRestaurantService(ctx);
        var inZone = await svc.GetByUserLocation(ZoneTestSeeder.InZoneLat, ZoneTestSeeder.InZoneLng, null);
        Assert.True(inZone.success);
        Assert.Single(Assert.IsAssignableFrom<IEnumerable<object>>(inZone.data));

        var outside = await svc.GetByUserLocation(35.0, 45.0, null);
        Assert.True(outside.success);
        Assert.Empty(Assert.IsAssignableFrom<IEnumerable<object>>(outside.data));
    }

    [Fact]
    public async Task GetByUserLocation_HidesRestaurant_WithoutAssignedZones()
    {
        var ctx = TestDbContext.New();
        var zoneId = ZoneTestSeeder.SeedZone(ctx);
        ZoneTestSeeder.SeedAvailableDriver(ctx, 8, zoneId);
        SeedRestaurant(ctx, 1, "مطعم بدون زون");
        ctx.ChangeTracker.Clear();

        var svc = NewRestaurantService(ctx);
        var res = await svc.GetByUserLocation(ZoneTestSeeder.InZoneLat, ZoneTestSeeder.InZoneLng, null);

        Assert.True(res.success);
        Assert.Empty(Assert.IsAssignableFrom<IEnumerable<object>>(res.data));
    }

    [Fact]
    public async Task GetByUserLocation_HidesAll_WhenNoDriverInCustomerZone()
    {
        var ctx = TestDbContext.New();
        var zoneA = ZoneTestSeeder.SeedZone(ctx, "ZoneA");
        var zoneB = ZoneTestSeeder.SeedZone(ctx, "ZoneB");
        ZoneTestSeeder.SeedAvailableDriver(ctx, 8, zoneB);

        var restaurant = SeedRestaurant(ctx, 1, "مطعم A");
        ZoneTestSeeder.AssignRestaurantToZone(ctx, restaurant.RestaurantId, zoneA);
        ctx.ChangeTracker.Clear();

        var svc = NewRestaurantService(ctx);
        var res = await svc.GetByUserLocation(ZoneTestSeeder.InZoneLat, ZoneTestSeeder.InZoneLng, null);

        Assert.True(res.success);
        Assert.Empty(Assert.IsAssignableFrom<IEnumerable<object>>(res.data));
    }
}
