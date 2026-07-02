namespace RomanaWeb.Helper.Interface
{
    public interface IOrderHubNotifier
    {
        Task NotifyRestaurantAsync(int restaurantId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode);
        Task NotifyUserAsync(int userId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode);
        Task NotifyDriverAsync(int driverId, string title, string message, int orderId, string statusKey, int statusCode, string displayMode);
        Task NotifyAllDriversAsync(string title, string message, int orderId, string statusKey, int statusCode, string displayMode);
        Task NotifyUserDriverLocationUpdatedAsync(int userId, int orderId, int saleManId, double lat, double lng, DateTime at);
    }
}
