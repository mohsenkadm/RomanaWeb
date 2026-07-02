using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.Entity
{
    public class Orders
    {
        public int OrderId { get; set; }
        public int OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public int RestaurantId { get; set; }
        public int UserId { get; set; }
        public double Total { get; set; }
        public double TotalDiscount { get; set; }
        public double NetAmount { get; set; }
        public bool IsCancel { get; set; }
        public bool IsApporve { get; set; }
        public bool IsDone { get; set; }
        public string? Notes { get; set; }
        public string? PromoCode { get; set; }    
        public int? SaleManId { get; set; }    
        public bool? IsSaleManApprove { get; set; }    
        public bool? IsSaleManCancel { get; set; }    
        public bool? IsDelivered { get; set; }    
        public bool? IsNotDelivered { get; set; }    
        public bool? IsWaiting { get; set; }
        public bool IsPreparing { get; set; }
        public bool IsDriverEnRouteToPickup { get; set; }
        public bool IsPickedUpFromRestaurant { get; set; }
        public bool IsOutForDelivery { get; set; }
        public bool IsDeliveryConfirmed { get; set; }
        public string? Reason { get; set; }
        public string? Reason2 { get; set; }
        public string? SaleManName { get; set; }
        /// <summary>Populated on order-detail API; not mapped to Orders table.</summary>
        public SaleMan? Driver { get; set; }

        public decimal? CostDelivery { get; set; }
        // for show
        public string? UserName { get; set; }
        public string? Phone { get; set; }      
        public string? ResPhone { get; set; }      
        public string? Address { get; set; }
        public string? FunctionPoint { get; set; }
        public string? Logo { get; set; }
        public string? Lat { get; set; }
        public string? Long { get; set; }
        /// <summary>API-only: restaurant pickup latitude (not persisted).</summary>
        public string? RestaurantLat { get; set; }
        /// <summary>API-only: restaurant pickup longitude (not persisted).</summary>
        public string? RestaurantLong { get; set; }
        /// <summary>API-only: customer dropoff latitude (duplicate of Lat for clarity).</summary>
        public string? DropoffLat { get; set; }
        /// <summary>API-only: customer dropoff longitude (duplicate of Long for clarity).</summary>
        public string? DropoffLng { get; set; }
        public string? RestaurantName { get; set; }
        public string? CategoriesName { get; set; }
        public string? CityName { get; set; }
        public string? CountriesName { get; set; }

        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
