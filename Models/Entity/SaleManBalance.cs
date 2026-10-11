namespace RomanaWeb.Models.Entity
{
    /// <summary>
    /// Running dual balances for a delivery agent (SaleMan):
    /// food net (بدون أجور توصيل) + delivery fees due.
    /// </summary>
    public class SaleManBalance
    {
        public int SaleManId { get; set; }
        public decimal FoodNetBalance { get; set; }
        public decimal DeliveryFeeBalance { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
