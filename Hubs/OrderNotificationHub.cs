using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using RomanaWeb.Classes;
using RomanaWeb.Model.General;

namespace RomanaWeb.Hubs
{
    /// <summary>
    /// Real-time order notifications for restaurants, customers, and drivers.
    /// Clients join groups: restaurant_{id}, user_{id}, driver_{id}, drivers_all
    /// Group ids must match the authenticated JWT identity.
    /// </summary>
    [Authorize]
    public class OrderNotificationHub : Hub
    {
        private UserManager? GetManager()
        {
            var raw = Context.User?.Claims
                .FirstOrDefault(x => x.Type == ClaimInfo.UserManager)?.Value;
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return JsonConvert.DeserializeObject<UserManager>(raw);
        }

        public async Task JoinRestaurant(int restaurantId)
        {
            var m = GetManager();
            if (m == null) throw new HubException("غير مصرح");
            bool ok = string.Equals(m.Role, "res", StringComparison.OrdinalIgnoreCase) && m.Id == restaurantId
                || string.Equals(m.Role, "admin", StringComparison.OrdinalIgnoreCase);
            if (!ok) throw new HubException("غير مصرح بالانضمام لهذه المجموعة");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");
        }

        public async Task JoinUser(int userId)
        {
            var m = GetManager();
            if (m == null) throw new HubException("غير مصرح");
            bool ok = string.Equals(m.Role, "user", StringComparison.OrdinalIgnoreCase) && m.Id == userId
                || string.Equals(m.Role, "admin", StringComparison.OrdinalIgnoreCase);
            if (!ok) throw new HubException("غير مصرح بالانضمام لهذه المجموعة");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task JoinDriver(int driverId)
        {
            var m = GetManager();
            if (m == null) throw new HubException("غير مصرح");
            bool ok = string.Equals(m.Role, "sal", StringComparison.OrdinalIgnoreCase) && m.Id == driverId
                || string.Equals(m.Role, "admin", StringComparison.OrdinalIgnoreCase);
            if (!ok) throw new HubException("غير مصرح بالانضمام لهذه المجموعة");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"driver_{driverId}");
        }

        public async Task JoinAllDrivers()
        {
            var m = GetManager();
            if (m == null) throw new HubException("غير مصرح");
            bool ok = string.Equals(m.Role, "sal", StringComparison.OrdinalIgnoreCase)
                || string.Equals(m.Role, "admin", StringComparison.OrdinalIgnoreCase);
            if (!ok) throw new HubException("غير مصرح");
            await Groups.AddToGroupAsync(Context.ConnectionId, "drivers_all");
        }

        public async Task LeaveRestaurant(int restaurantId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");

        public async Task LeaveUser(int userId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

        public async Task LeaveDriver(int driverId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"driver_{driverId}");
    }
}
