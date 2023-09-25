using RomanaWeb.Models.Entity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.EntityMapper
{
    public class ProductsModel
    {
        public int ProductsId { get; set; }
        public string ProductsName { get; set; }
        public string ProductsDetails { get; set; }
        public decimal ProductsPrice { get; set; }
        public int RestaurantId { get; set; }
        public int SubCategoriesId { get; set; }   
        public IFormFile FileChoose { get; set; }
    }
}
