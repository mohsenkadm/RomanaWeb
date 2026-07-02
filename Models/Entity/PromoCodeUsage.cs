namespace RomanaWeb.Models.Entity
{
    public class PromoCodeUsage
    {
        public int PromoCodeUsageId { get; set; }
        public int PromoCodeId { get; set; }
        public int UserId { get; set; }
        public int UsedCount { get; set; }
    }
}
