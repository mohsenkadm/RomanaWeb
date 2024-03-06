using RomanaWeb.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.EntityMapper
{
    public class OrdersModel
    {                                         
        public int RestaurantId { get; set; }
        public int UserId { get; set; }
        public double Total { get; set; }
        public double TotalDiscount { get; set; }
        public double NetAmount { get; set; }
        public string Notes { get; set; }
        public string PromoCode { get; set; }

        public decimal? CostDelivery { get; set; }

        //for user      

        public Users   Users { get; set; }
        public List<OrderDetailModel> OrderDetails { get; set; }
    }
}
