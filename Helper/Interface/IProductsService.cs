using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IProductsService
    {
        Task<ResObj> GetAll(string? Name, string? RestaurantName, string? SubCategoriesName,int index);
        Task<ResObj> GetByRestaurantId(int RestaurantId, int UserId,int? SubCategoriesId);
        Task<ResObj> Post(Products Products);  
        Task<ResObj> Update(Products Products);
        Task<ResObj> Delete(int Id);
        Task<Products> GetProductsById(int Id);
        Task<ResObj> GetById(int Id);             
    }
}
