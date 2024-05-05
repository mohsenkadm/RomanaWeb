using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IProductsService
    {
        Task<ResObj> GetAll(string? Name, string? RestaurantName, string? SubCategoriesName,int index);
        Task<ResObj> GetAllBySearch(string? Name, int index);
        Task<ResObj> GetByRestaurantId(int RestaurantId, int? SubCategoriesId, string? prodname);
        Task<ResObj> Post(Products Products);  
        Task<ResObj> Update(Products Products);
        Task<ResObj> Delete(int Id);
        Task<Products> GetProductsById(int Id);
        Task<ResObj> GetById(int Id);
        Task<ResObj> SetIsFree(int id, bool free);

        Task<ResObj> DeleteImage(int id);
        Task<ResObj> DeleteImageForProd(int id);
        Task<ResObj> PostImages(Images images);
        Task<ResObj> GetImagesByProductsId(int Id);
    }
}
