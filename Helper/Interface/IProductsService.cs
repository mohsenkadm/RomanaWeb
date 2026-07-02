using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;
using RomanaWeb.Models.EntityMapper;

namespace RomanaWeb.Helper.Interface
{
    public interface IProductsService
    {
        Task<ResObj> PostBulk(QuickProductsBulkRequest request, string placeholderImageUrl);
        Task<ResObj> GetAll(string? Name, string? RestaurantName, string? SubCategoriesName,int index);
        Task<ResObj> GetAllBySearch(string? Name, int index);
        Task<ResObj> GetByRestaurantId(int RestaurantId, int? SubCategoriesId, string? prodname);
        Task<ResObj> Post(Products Products);  
        Task<ResObj> Update(Products Products);
        Task<ResObj> Delete(int Id);
        Task<Products> GetProductsById(int Id);
        Task<ResObj> GetById(int Id);
        Task<ResObj> SetIsFree(int id, bool free);
        Task<ResObj> SetIsAvailable(int id, bool isAvailable);
        Task<ResObj> GetTopSellingByRestaurant(int restaurantId, int take = 20);

        Task<ResObj> DeleteImage(int id);
        Task<ResObj> DeleteImageForProd(int id);
        Task<ResObj> PostImages(Images images);
        Task<ResObj> GetImagesByProductsId(int Id);

        // Product sizes
        Task<ResObj> GetSizesByProductId(int productId);
        Task<ResObj> PostSize(ProductSize size);
        Task<ResObj> DeleteSize(int sizeId);

        // Product ingredients
        Task<ResObj> GetIngredientsByProductId(int productId);
        Task<ResObj> PostIngredient(ProductIngredient ingredient);
        Task<ResObj> DeleteIngredient(int ingredientId);
    }
}
