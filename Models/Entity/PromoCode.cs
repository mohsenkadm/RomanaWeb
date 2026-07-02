using System;
using System.ComponentModel.DataAnnotations.Schema;

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

        // --- New fields (Section 3) ---

        /// <summary>"Percentage" or "Fixed". Optional; if null we infer from Percentage/DiscountAmount.</summary>
        public string DiscountType { get; set; }

        /// <summary>Admin-defined ceiling. 0 = no cap.</summary>
        public decimal MaxDiscountAmount { get; set; }

        /// <summary>First time this code was successfully applied to an order. Null until first use.</summary>
        public DateTime? FirstUsedAt { get; set; }

        /// <summary>Max times a single user may apply this code (0 = unlimited).</summary>
        public int MaxUsagePerUser { get; set; } = 1;

        // --- Aliases mapped to existing columns (kept NotMapped so EF ignores them) ---

        [NotMapped]
        public string Scope
        {
            get => IsForAllStores ? "GLOBAL" : "STORE";
            set => IsForAllStores = string.Equals(value, "GLOBAL", StringComparison.OrdinalIgnoreCase);
        }

        [NotMapped]
        public int UsageLimit
        {
            get => MaxOrders;
            set => MaxOrders = value;
        }

        [NotMapped]
        public int UsageCount
        {
            get => UsedOrders;
            set => UsedOrders = value;
        }

        /// <summary>Store-scoped discounts are charged against the store, not the platform.</summary>
        [NotMapped]
        public bool FundedByStore => !IsForAllStores;

        /// <summary>Resolved discount value (uses DiscountValue setter or falls back to legacy fields).</summary>
        [NotMapped]
        public decimal DiscountValue
        {
            get
            {
                if (string.Equals(DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
                    return Percentage;
                if (string.Equals(DiscountType, "Fixed", StringComparison.OrdinalIgnoreCase))
                    return DiscountAmount;
                // Legacy fallback
                return DiscountAmount > 0 ? DiscountAmount : Percentage;
            }
            set
            {
                if (string.Equals(DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
                    Percentage = (int)value;
                else
                    DiscountAmount = value;
            }
        }
    }
}
