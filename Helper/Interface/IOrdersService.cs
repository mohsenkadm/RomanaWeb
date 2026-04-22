using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IOrdersService
    {
        Task<ResObj> SetIsSaleManApprove(int id);
        Task<ResObj> SetIsSaleManCancel(int id);
        Task<ResObj> SetIsNotDelivered(int id,string Reason);
        Task<ResObj> SetIsWaiting(int id,string Reason2);
        Task<ResObj> SetIsDelivered(int id);
        Task<ResObj> SetSaleManId(int id, int SaleManId);
        Task<ResObj> SetIsApporve(int id);
        Task<ResObj> SetIsCancel(int id);
        Task<ResObj> SetIsDone(int id);
        Task<ResObj> Delete(int Id);
        Task<ResObj> GetById(int Id);        
        Task<ResObj> GetAll(string? orderNo, string? UserName, DateTime datefrom, DateTime dateto, int? RestaurantId,int? CountriesId,int? state);
        Task<ResObj> GetOrdersWithDetailAll(int Id);
        Task<ResObj> Post(Orders Orders);
        Task<ResObj> DeleteDetails(int id);
        Task<ResObj> GetOrdersByOrderNoAndUserId(string orderNo, int? id);
        Task<ResObj> GetOrdersByOrderNoAndRestaurantId(string orderNo, int? RestaurantId,int Type);
        Task<ResObj> GetOrdersByOrderNoAndSaleManId(string? OrderNo, int? SaleManId, int Type);
        Task<ResObj> PostUser(Users users);
        Task<string> GetNamePersonById(int userId);
        Task<string> GetSaleManPersonById(int SaleManId);
        Task<ResObj> ModifyOrder(int orderId, List<OrderDetail> newDetails);
        Task<ResObj> AddOrderDetail(OrderDetail detail);
        Task<ResObj> GetNearbyDriverOrders(int saleManId, double lat, double lng, double radiusKm);
        Task<ResObj> ApproveOrderBySaleMan(int orderId, int saleManId);
    }
}
