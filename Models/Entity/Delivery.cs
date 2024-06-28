namespace RomanaWeb.Models.Entity
{
    public class Delivery
    {
        public int DeliveryId { get; set; }
        public int? No { get; set; }           
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? FunctionPoint { get; set; }     
        public string? RestaurantName { get; set; }
        public decimal? CostDelivery { get; set; }
        public string? Notes { get; set; }
        public decimal NetAmount { get; set; }
        public int RestaurantId { get; set; }
        public int CityId { get; set; }  
        public int CountriesId { get; set; }
        public string CityName { get; set; }
        public string CountriesName { get; set; }
        public DateTime DateInsert { get; set; }

        public bool? IsDelivered { get; set; }
        public bool? IsNotDelivered { get; set; }
        public bool? IsWaiting { get; set; }
        public string? Reason { get; set; }
        public string? Reason2 { get; set; }
        public string? ResPhone { get; set; }
    }
}
