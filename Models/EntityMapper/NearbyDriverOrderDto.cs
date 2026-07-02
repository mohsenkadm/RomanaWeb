namespace RomanaWeb.Models.EntityMapper
{
    public class NearbyDriverOrderDto
    {
        public int OrderId { get; set; }
        public int OrderNo { get; set; }
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public double PickupLat { get; set; }
        public double PickupLong { get; set; }
        public double DropoffLat { get; set; }
        public double DropoffLong { get; set; }
        public double DistanceToPickupKm { get; set; }
        public double PickupToDropoffKm { get; set; }
        public double DistanceToDropoffKm { get; set; }
        public decimal EstimatedFee { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
    }
}
