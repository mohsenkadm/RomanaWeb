namespace RomanaWeb.Models.Entity
{
    public class RestaurantCity
    {
        public int RestaurantCityId { get; set; }
        public int CityId { get; set; }
        public int RestaurantId { get; set; }
        public decimal? CostDelivery { get; set; }
        // for show
        public string? CityName { get; set; }
        public string? RestaurantName { get; set; }
    }
}
