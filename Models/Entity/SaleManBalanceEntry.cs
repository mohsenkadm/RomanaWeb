using System.ComponentModel.DataAnnotations.Schema;

namespace RomanaWeb.Models.Entity
{
    /// <summary>
    /// One accrual per delivered order. SettlementId null = still in open balance.
    /// </summary>
    public class SaleManBalanceEntry
    {
        public int EntryId { get; set; }
        public int SaleManId { get; set; }
        public int OrderId { get; set; }
        public decimal FoodNetAmount { get; set; }
        public decimal DeliveryFeeAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? SettlementId { get; set; }

        [NotMapped] public int? OrderNo { get; set; }
        [NotMapped] public DateTime? OrderDate { get; set; }
        [NotMapped] public string? RestaurantName { get; set; }
        [NotMapped] public string? UserName { get; set; }
        [NotMapped] public string? Address { get; set; }
    }
}
