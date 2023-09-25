using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IOrdersService
    {
        Task<ResObj> SetIsApporve(int id);
        Task<ResObj> SetIsCancel(int id);
        Task<ResObj> SetIsDone(int id);
        Task<ResObj> Delete(int Id);
        Task<ResObj> GetById(int Id);        
        Task<ResObj> GetAll(string? orderNo, string? UserName, DateTime datefrom, DateTime dateto);
        Task<ResObj> GetOrdersWithDetailAll(int Id);
        Task<ResObj> Post(Orders Orders);
        Task<ResObj> DeleteDetails(int id);
        Task<ResObj> GetOrdersByOrderNoAndUserId(string orderNo, int? id);
        Task<ResObj> GetOrdersByOrderNoAndRestaurantId(string orderNo, int? RestaurantId);
        Task<ResObj> PostUser(Users users);
    }
}
