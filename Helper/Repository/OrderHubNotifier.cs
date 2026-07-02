using Microsoft.AspNetCore.SignalR;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Hubs;

namespace RomanaWeb.Helper.Repository
{
    public class OrderHubNotifier : IOrderHubNotifier, IRegisterScopped
    {
        private readonly IHubContext<OrderNotificationHub> _hub;

        public OrderHubNotifier(IHubContext<OrderNotificationHub> hub) => _hub = hub;

        public Task NotifyRestaurantAsync(int restaurantId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode) =>
            _hub.Clients.Group($"restaurant_{restaurantId}")
                .SendAsync("OrderUpdated", Payload(title, message, orderId, statusKey, statusCode, displayMode, "restaurant", restaurantId));

        public Task NotifyUserAsync(int userId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode) =>
            _hub.Clients.Group($"user_{userId}")
                .SendAsync("OrderUpdated", Payload(title, message, orderId, statusKey, statusCode, displayMode, "user", userId));

        public Task NotifyDriverAsync(int driverId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode) =>
            _hub.Clients.Group($"driver_{driverId}")
                .SendAsync("OrderUpdated", Payload(title, message, orderId, statusKey, statusCode, displayMode, "driver", driverId));

        public Task NotifyAllDriversAsync(string title, string message, int orderId, string statusKey, int statusCode, string displayMode) =>
            _hub.Clients.Group("drivers_all")
                .SendAsync("OrderUpdated", Payload(title, message, orderId, statusKey, statusCode, displayMode, "drivers_all", 0));

        public Task NotifyUserDriverLocationUpdatedAsync(int userId, int orderId, int saleManId, double lat, double lng, DateTime at) =>
            _hub.Clients.Group($"user_{userId}")
                .SendAsync("DriverLocationUpdated", new { orderId, saleManId, lat, lng, at });

        private static object Payload(string title, string message, int orderId, string statusKey, int statusCode, string displayMode, string audience, int audienceId) =>
            new { title, message, orderId, statusKey, statusCode, displayMode, audience, audienceId, at = DateTime.UtcNow };
    }
}
