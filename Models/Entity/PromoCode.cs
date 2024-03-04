namespace RomanaWeb.Models.Entity
{
    public class PromoCode
    {
        public int PromoCodeId { get; set; }
        public string PromoName { get; set; }
        public int Percentage { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
    }
}
