using System.ComponentModel.DataAnnotations;

namespace RomanaWeb.Models.Entity
{
    public class Stars
    {                    
        public int StarsId { get; set; }
        [Required]
        public int? UserId { get; set; }
        [Required]
        [Range(0,5)]
        public int? StarsCount { get; set; }
        [Required]
        public int? RestaurantId { get; set; }     
        [Required]
        [StringLength(int.MaxValue)]
        public string? Comments { get; set; }    
        public string? UserName { get; set; }
        public string? RestaurantName { get; set; }    
    }
}
