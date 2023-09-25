using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IStarsService
    {                                              
        public Task<ResObj> GetByRestaurantId(int RestaurantId);
        public Task<ResObj> GetAll(string? RestaurantName,int index);
        public Task<ResObj> GetById(int Id);
        public Task<ResObj> Delete(int Id);
        public Task<ResObj> Post(Stars stars);  
    }
}
