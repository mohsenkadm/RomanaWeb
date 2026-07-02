using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.Entity
{
    public class Carousel
    {
        public int CarouseId { get; set; }
        public string Image { get; set; }
        public string? Url { get; set; }
        public bool IsShow { get; set; }
        public int CountryId { get; set; }
        /// <summary>1 = customer, 2 = restaurant, 3 = delivery</summary>
        public int AppType { get; set; } = 1;
        public int CountryName { get; set; }
    }
}
