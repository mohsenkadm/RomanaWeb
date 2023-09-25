using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.EntityMapper
{
    public class CarouselModel
    {
        public int CarouseId { get; set; }
        public IFormFile FileChoose { get; set; }
        public bool IsShow { get; set; }
        public string Url { get; set; }
    }
}
