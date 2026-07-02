using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.Entity
{
    public class SaleMan
    {
        public int SaleManId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }      
        public string Token { get; set; }        
        public string? Address { get; set; }    
        public string? Password { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }

        // NOTE: SaleMan is no longer joined to a restaurant.
        // The salesman is managed exclusively by the admin (add/edit/remove).
        // Kept as nullable for backwards compatibility with the existing DB column / SP outputs.
        public int? RestaurantId { get; set; }
        public string? RestaurantName { get; set; }

        // Driver "working/stopped" toggle. true = on shift, receives dispatch
        // notifications. false = off shift, dispatcher will skip them.
        // Default true so existing rows keep behaving as before.
        public bool IsAvailable { get; set; } = true;
        public DateTime? AvailabilityChangedAt { get; set; }

        // Section 6: driver location heartbeat - used by proximity-based dispatch.
        public string? Lat { get; set; }
        public string? Long { get; set; }
        public DateTime? LocationUpdatedAt { get; set; }
        // for total
        public decimal CountOrder { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalCostDelivery { get; set; }
        public decimal Total { get; set; }
        public decimal daCountOrder { get; set; }
        public decimal daNetAmount { get; set; }
        public decimal daCostDelivery { get; set; }
        public decimal daTotal { get; set; }
    }
}
