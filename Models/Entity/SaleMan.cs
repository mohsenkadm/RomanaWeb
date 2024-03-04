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
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
    }
}
