namespace RomanaWeb.Models.Entity
{
    public class RestaurantSubCategories
    {
        public int RestaurantSubCategoriesId { get; set; }
        public int SubCategoriesId { get; set; }
        public int RestaurantId { get; set; }
        // for show
        public string? SubCategoriesName { get; set; }
        public string? RestaurantName { get; set; }
    }
}
