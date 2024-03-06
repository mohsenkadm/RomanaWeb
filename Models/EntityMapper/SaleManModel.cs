namespace RomanaWeb.Models.EntityMapper
{
    public class SaleManModel
    {
        public int SaleManId { get; set; }
        public string? Name { get; set; }
        public string? Details { get; set; }
        public string? Address { get; set; }      
        public string? Phone { get; set; }   
        public string? Password { get; set; }   
        public bool? IsActive { get; set; }
        public int RestaurantId { get; set; }
    }
}
