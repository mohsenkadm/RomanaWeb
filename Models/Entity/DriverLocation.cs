namespace RomanaWeb.Models.Entity
{
    public class DriverLocation
    {
        public int SaleManId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? ActiveOrderId { get; set; }
    }
}
