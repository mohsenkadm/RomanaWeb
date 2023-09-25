namespace RomanaWeb.Models.EntityMapper
{
    public class RestaurantModel
    {
        public int RestaurantId { get; set; }
        public string? Name { get; set; }
        public string? Details { get; set; }
        public string? Address { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? Background { get; set; }
        public string? Phone { get; set; }
        public string? Lat { get; set; }
        public string? Long { get; set; }
        public string? Code { get; set; }
        public string? Whatsapp { get; set; }
        public string? Password { get; set; }
        public string? UserName { get; set; } 
        public int? StarCount { get; set; }
        public bool? IsClosed { get; set; }
        public bool? IsStars { get; set; }
        public decimal MinimumPrice { get; set; }
        public string Areaname { get; set; }
        public int CategoriesId { get; set; }    
    }
}
