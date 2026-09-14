using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IRestaurantSaleManService
    {
        public Task<ResObj> GetAll(string? Name);
        public Task<ResObj> GetById(int Id);
        public Task<ResObj> GetByResId(int Id);
        public Task<ResObj> GetBySaleManId(int saleManId);
        public Task<ResObj> SetSaleManRestaurants(int saleManId, int[] restaurantIds);
        public Task<ResObj> LinksSummary();
        public Task<ResObj> Delete(int Id);
        public Task<ResObj> Post(RestaurantSaleMan RestaurantSaleMan);
    }
}
