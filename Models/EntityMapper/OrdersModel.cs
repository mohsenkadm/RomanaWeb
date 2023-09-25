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
        public DateTime OrderDate { get; set; }
        public int ShopId { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetAmount { get; set; }

        //for user

        public Users   Users { get; set; }
        public List<OrderDetailModel> OrderDetails { get; set; }
    }
}
