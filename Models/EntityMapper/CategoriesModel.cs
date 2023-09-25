using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.EntityMapper
{
    public class CategoriesModel
    {                    
        public int CategoriesId { get; set; }
        public string? CategoriesName { get; set; }   
        public IFormFile FileChoose { get; set; }
    }
}
