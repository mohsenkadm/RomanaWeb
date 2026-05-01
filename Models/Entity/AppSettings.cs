namespace RomanaWeb.Models.Entity
{
    public class AppSettings
    {
        public int AppSettingsId { get; set; }
        public decimal PricePerKm { get; set; }
        public decimal DefaultOrderCost { get; set; }

        // Section 2.1: minimum-charge rule (configurable by admin).
        public decimal MinChargeKmThreshold { get; set; } = 1.5m;
        public decimal MinChargeAmount { get; set; } = 500m;

        // Section 2.1: rounding rule. Allowed: "Ceil", "Round", "Floor". Default Ceil.
        public string RoundingMode { get; set; } = "Ceil";

        // Section 2.2 (zones): max km billed per zone; 0 = no cap.
        public decimal ZoneMaxKm { get; set; } = 0m;
        public decimal ZoneMinKm { get; set; } = 0m;
    }
}
