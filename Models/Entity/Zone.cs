namespace RomanaWeb.Models.Entity
{
    /// <summary>
    /// Delivery zone (polygon) with LZA/ECA pricing per PDF Rev.0001.
    /// GeoJSON Polygon coordinates in [lng, lat] order.
    /// </summary>
    public class Zone
    {
        public int ZoneId { get; set; }
        public string Name { get; set; } = "";
        public string GeoJson { get; set; } = "";
        public bool IsActive { get; set; } = true;

        /// <summary>Default delivery price within/to this zone (IQD).</summary>
        public decimal? BaseDeliveryPrice { get; set; }

        /// <summary>Limited Zone Area — free distance km inside zone before ECA.</summary>
        public decimal LzaKm { get; set; } = 3m;

        /// <summary>Extra Charge Area price per km (IQD).</summary>
        public decimal EcaPricePerKm { get; set; } = 250m;

        /// <summary>Maximum ECA fee that can be added on top of zone price.</summary>
        public decimal MaxEcaFee { get; set; } = 2500m;

        /// <summary>Maximum total delivery fee (zone + ECA). When set, final price never exceeds this.</summary>
        public decimal? MaxTotalDeliveryFee { get; set; }

        /// <summary>Delivery price when customer is within NearRestaurantKm of restaurant.</summary>
        public decimal? NearRestaurantPrice { get; set; }

        public decimal NearRestaurantKm { get; set; } = 1m;
    }

    public class ZonePrice
    {
        public int ZonePriceId { get; set; }
        public int FromZoneId { get; set; }
        public int ToZoneId { get; set; }
        public decimal Price { get; set; }
    }

    public class RestaurantZone
    {
        public int RestaurantZoneId { get; set; }
        public int RestaurantId { get; set; }
        public int ZoneId { get; set; }
    }

    public class SaleManZone
    {
        public int SaleManZoneId { get; set; }
        public int SaleManId { get; set; }
        public int ZoneId { get; set; }
    }

    public class ServiceCoverageRequest
    {
        public int ServiceCoverageRequestId { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Address { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsProcessed { get; set; }
    }
}
