using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IDeliveryService
    {
        public Task<ResObj> GetAll(string? Name, int? CountriesId, string? RestaurantName
            , DateTime datefrom, DateTime dateto, int? No=0);               
        public Task<ResObj> GetById(int Id);
        public Task<ResObj> Delete(int Id);
        public Task<ResObj> Post(Delivery Delivery);
        public Task<ResObj> GetDeliveryByNoAndRestaurantId(int? no, int? restaurantId);

        Task<ResObj> SetIsNotDelivered(int id, string Reason);
        Task<ResObj> SetIsWaiting(int id, string Reason2);
        Task<ResObj> SetIsDelivered(int id);
    }
}
