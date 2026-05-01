namespace RomanaWeb.Models.Entity
{
    /// <summary>
    /// Section 2.2 - A delivery zone (polygon) used by the pricing engine.
    /// Polygon is stored as GeoJSON ("Polygon" geometry, coordinates in [lng, lat] order).
    /// </summary>
    public class Zone
    {
        public int ZoneId { get; set; }
        public string Name { get; set; } = "";
        /// <summary>GeoJSON Polygon as raw text. Example: { "type":"Polygon", "coordinates":[ [ [lng,lat], ... ] ] }</summary>
        public string GeoJson { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Section 2.2 - Zone-to-zone price matrix entry. From zone A → to zone B = Price IQD.
    /// </summary>
    public class ZonePrice
    {
        public int ZonePriceId { get; set; }
        public int FromZoneId { get; set; }
        public int ToZoneId { get; set; }
        public decimal Price { get; set; }
    }
}
