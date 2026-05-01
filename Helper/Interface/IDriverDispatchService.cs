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
        Task<ResObj> UpdateDriverLocation(int saleManId, double lat, double lng);
    }
}
