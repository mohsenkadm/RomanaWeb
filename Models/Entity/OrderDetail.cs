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
        public double Price { get; set; }
        public int Count { get; set; }                    
        public string? Notes2 { get; set; }
        // Size & ingredients selected by the customer at order time
        public int? SelectedSizeId { get; set; }
        public string? SelectedSizeName { get; set; }
        public double? SelectedSizePrice { get; set; }
        public string? SelectedIngredients { get; set; } // comma-separated ingredient names                
        // for show
        public string ProductsName { get; set; }     
        public string ProductsDetails { get; set; }     
        public string ProductsImage { get; set; }
        public string SubCategoriesName { get; set; }
        public int OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public int RestaurantId { get; set; }
        public int UserId { get; set; }
        public double Total { get; set; }
        public double TotalDiscount { get; set; }
        public double NetAmount { get; set; }
        public decimal? CostDelivery { get; set; }
        public bool IsCancel { get; set; }
        public bool IsApporve { get; set; }
        public bool IsDone { get; set; }
    }
}
