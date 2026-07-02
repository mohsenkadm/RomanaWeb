using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RomanaWeb.Hubs
{
    /// <summary>
    /// Real-time order notifications for restaurants, customers, and drivers.
    /// Clients join groups: restaurant_{id}, user_{id}, driver_{id}, drivers_all
    /// </summary>
    [Authorize]
    public class OrderNotificationHub : Hub
    {
        public async Task JoinRestaurant(int restaurantId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");

        public async Task JoinUser(int userId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        public async Task JoinDriver(int driverId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, $"driver_{driverId}");

        public async Task JoinAllDrivers() =>
            await Groups.AddToGroupAsync(Context.ConnectionId, "drivers_all");

        public async Task LeaveRestaurant(int restaurantId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");

        public async Task LeaveUser(int userId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

        public async Task LeaveDriver(int driverId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"driver_{driverId}");
    }
}
