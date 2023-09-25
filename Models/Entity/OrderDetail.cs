using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.Entity
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductsId { get; set; }       
        public decimal Price { get; set; }
        public int Count { get; set; }                    
        // for show
        public string ProductsName { get; set; }     
        public string ProductsDetails { get; set; }     
        public string ProductsImage { get; set; }
        public string SubCategoriesName { get; set; }
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
    }
}
