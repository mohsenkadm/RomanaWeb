namespace RomanaWeb.Models.Entity
{
    public class Delivery
    {
        public int DeliveryId { get; set; }
        public string No { get; set; }           
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? FunctionPoint { get; set; }

        public string? RestaurantName { get; set; }
        public decimal? CostDelivery { get; set; }
        public string? Notes { get; set; }
        public double NetAmount { get; set; }
        public int RestaurantId { get; set; }
        public int CityId { get; set; }  
        public int CountriesId { get; set; }
        public string CityName { get; set; }
        public string CountriesName { get; set; }
        public DateTime DateInsert { get; set; }
    }
}
