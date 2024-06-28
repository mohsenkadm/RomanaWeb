using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.Entity
{
    public class Users
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }      
        public string Token { get; set; }        
        public string? Address { get; set; }    
        public string FunctionPoint { get; set; }
        public string? Lat { get; set; }
        public string? Long { get; set; }
        public string? Password { get; set; }
        public bool? IsConfirm { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
        public string? Code { get; set; }
        public int? CityId { get; set; }
    }
}
