using Microsoft.AspNetCore.Http;

namespace RomanaWeb.Models.Entity
{
    public class Products
    {
        public int ProductsId { get; set; }
        public string? ProductsName { get; set; }
        public string? ProductsDetails { get; set; }
        public double? ProductsPrice { get; set; }    
        public int? RestaurantId { get; set; }
        public int? SubCategoriesId { get; set; }   
        public string? SubCategoriesName { get; set; }
        public string? RestaurantName { get; set; }
        public string? Logo { get; set; }
        public string? ProductsImageFirst { get; set; }      
        public string? Background { get; set; }
        public bool? IsFree { get; set; }
        public int PreparationTimeMinutes { get; set; } = 15;
        public bool IsAvailable { get; set; } = true;
        public List<Images> Images { get; set; }
        public List<ProductSize>? Sizes { get; set; }
        public List<ProductIngredient>? Ingredients { get; set; }
    }
}
