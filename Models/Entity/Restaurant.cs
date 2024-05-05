using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.Entity
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string? Name { get; set; }
        public string? Details { get; set; }
        public string? Address { get; set; }   
        public string? Logo { get; set; }
        public string? Background { get; set; }
        public string? Phone { get; set; }
        public string? Lat { get; set; }
        public string? Long { get; set; }        
        public string? Code { get; set; }  
        public string? Whatsapp { get; set; }  
        public string? Password { get; set; }
        public string? UserName { get; set; }
        public string? Token { get; set; }    
        public int? StarCount { get; set; }
        public bool? IsClosed { get; set; }
        public bool? IsStars { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsTop { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsApproved { get; set; }
        public decimal MinimumPrice { get; set; }
        public string? Areaname { get; set; }
        public decimal? CostDelivery { get; set; }
        public int CategoriesId { get; set; }
        public string CategoriesName { get; set; }
        public string Insta { get; set; }

        // for total
        public int CountProducts { get; set; }
        public decimal TotalNetDelivery { get; set; }
        public int CountDelivery { get; set; }
        public int CountOrder { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal Total { get; set; }
        public decimal daCountOrder { get; set; }
        public decimal daNetAmount { get; set; }
        public decimal daTotalDiscount { get; set; }
        public decimal daTotal { get; set; }
    }
}
