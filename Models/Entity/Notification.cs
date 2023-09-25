using System;
using System.Collections.Generic;
using System.Text;

namespace RomanaWeb.Models.Entity
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string Details { get; set; }
        public string Title { get; set; }
        public string Images { get; set; }
        public IFormFile FileChoose { get; set; }
        public DateTime DateInsert { get; set; }  
        public int UserId { get; set; }  
        public int ResId { get; set; }  
    }
}
