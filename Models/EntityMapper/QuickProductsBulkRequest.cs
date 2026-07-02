namespace RomanaWeb.Models.EntityMapper
{
    public class QuickProductsBulkRequest
    {
        public int RestaurantId { get; set; }
        public int? DefaultSubCategoriesId { get; set; }
        public int DefaultPreparationTimeMinutes { get; set; } = 15;
        public List<QuickProductItem> Items { get; set; } = new();
    }

    public class QuickProductItem
    {
        public string ProductsName { get; set; } = "";
        public decimal ProductsPrice { get; set; }
        public int? SubCategoriesId { get; set; }
        public string? ProductsDetails { get; set; }
        public string? Base64Image { get; set; }
        public string? ImageUrl { get; set; }
    }
}
