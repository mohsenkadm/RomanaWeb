using System.ComponentModel.DataAnnotations.Schema;

namespace RomanaWeb.Models.Entity
{
    /// <summary>
    /// تصفير حساب المندوب — snapshot of both balances at settle time.
    /// Driver profile/orders remain; only balances go to zero.
    /// </summary>
    public class SaleManSettlement
    {
        public int SettlementId { get; set; }
        public int SaleManId { get; set; }
        public decimal FoodNetAmount { get; set; }
        public decimal DeliveryFeeAmount { get; set; }
        public DateTime SettledAt { get; set; }
        public int? SettledByAdminId { get; set; }
        public string? Note { get; set; }

        [NotMapped] public string? SaleManName { get; set; }
        [NotMapped] public string? SaleManPhone { get; set; }
        [NotMapped] public string? AdminName { get; set; }
        [NotMapped] public decimal CurrentFoodNetBalance { get; set; }
        [NotMapped] public decimal CurrentDeliveryFeeBalance { get; set; }
        [NotMapped] public int OrdersCount { get; set; }
    }
}
