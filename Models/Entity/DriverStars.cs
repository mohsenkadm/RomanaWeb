using System.ComponentModel.DataAnnotations;

namespace RomanaWeb.Models.Entity
{
    public class DriverStars
    {
        public int DriverStarsId { get; set; }
        [Required]
        [Range(0, 5)]
        public int? StarsCount { get; set; }
        [Required]
        public int? SaleManId { get; set; }
        [Required]
        [StringLength(int.MaxValue)]
        public string? Comments { get; set; }
        public int? OrderId { get; set; }
        public string? UserName { get; set; }
        public string? SaleManName { get; set; }
    }
}
