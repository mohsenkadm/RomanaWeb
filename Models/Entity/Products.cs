using Microsoft.AspNetCore.Http;

namespace RomanaWeb.Models.Entity
{
    public class Products
    {
        public int ProductsId { get; set; }
        public string? ProductsName { get; set; }
        public string? ProductsDetails { get; set; }
        public decimal? ProductsPrice { get; set; }    
        public int RestaurantId { get; set; }
        public int SubCategoriesId { get; set; }   
        public string? SubCategoriesName { get; set; }
        public string? RestaurantName { get; set; }
        public string? ProductsImage { get; set; }      
    }
}
