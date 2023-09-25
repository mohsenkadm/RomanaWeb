using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.EntityMapper
{
    public class OrderDetailModel
    {       
        public int ProductsId { get; set; }          
        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}
