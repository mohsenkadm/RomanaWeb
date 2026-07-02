using RomanaWeb.Classes;

namespace RomanaWeb.Helper.Interface
{
    /// <summary>
    /// Section 6 - Driver dispatch service.
    /// - Pushes orders to nearby drivers ranked by proximity to pickup.
    /// - First-accept-wins is enforced by OrdersService.ApproveOrderBySaleMan.
    /// - On driver cancel: append reason to order notes and re-dispatch.
    /// </summary>
    public interface IDriverDispatchService
    {
        Task<ResObj> DispatchOrder(int orderId, double radiusKm = 5d);
        Task<ResObj> CancelByDriver(int orderId, int saleManId, string reason);
        Task<ResObj> UpdateDriverLocation(int saleManId, double lat, double lng, int? orderId = null);
        Task<(bool ok, int httpStatus, object? data, string? msg)> GetDriverLocationForOrder(int orderId, int userId);
        Task SetActiveOrderAsync(int saleManId, int orderId);
        Task ClearActiveOrderAsync(int saleManId);
        Task<bool> DriverHasActiveOrderAsync(int saleManId, int? excludeOrderId = null);
    }
}
