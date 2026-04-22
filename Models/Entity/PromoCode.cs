namespace RomanaWeb.Models.Entity
{
    public class PromoCode
    {
        public int PromoCodeId { get; set; }
        public string PromoName { get; set; }
        public int Percentage { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public int MaxOrders { get; set; }
        public int UsedOrders { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsForAllStores { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
