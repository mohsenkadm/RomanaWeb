using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IDeliveryService
    {
        public Task<ResObj> GetAll(string? Name, int? CountriesId, string? RestaurantName
            , DateTime datefrom, DateTime dateto, string? No);               
        public Task<ResObj> GetById(int Id);
        public Task<ResObj> Delete(int Id);
        public Task<ResObj> Post(Delivery Delivery);
        public Task<ResObj> GetDeliveryByNoAndRestaurantId(string? no, int? restaurantId);
    }
}
