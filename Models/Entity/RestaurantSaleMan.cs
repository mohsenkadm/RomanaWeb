namespace RomanaWeb.Models.Entity
{
    public class RestaurantSaleMan
    {
        public int RestaurantSaleManId { get; set; }
        public int SaleManId { get; set; }
        public int RestaurantId { get; set; }
        // for show
        public string? SaleManName { get; set; }
        public string? RestaurantName { get; set; }
    }
}
