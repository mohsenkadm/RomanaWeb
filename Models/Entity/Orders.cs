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
        public decimal Total { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetAmount { get; set; }
        public bool IsCancel { get; set; }
        public bool IsApporve { get; set; }
        public bool IsDone { get; set; }

        // for show
        public string UserName { get; set; }
        public string Phone { get; set; }      
        public string? Address { get; set; }
        public string FunctionPoint { get; set; }
        public string Logo { get; set; }
        public string RestaurantName { get; set; }
        public string CategoriesName { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}
