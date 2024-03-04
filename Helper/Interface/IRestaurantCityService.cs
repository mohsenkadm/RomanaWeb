using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IRestaurantCityService
    {
        public Task<ResObj> GetAll(string? Name);
        public Task<ResObj> GetById(int Id);
        public Task<ResObj> GetByResId(int Id);
        public Task<ResObj> Delete(int Id);
        public Task<ResObj> Post(RestaurantCity RestaurantCity);
    }
}
