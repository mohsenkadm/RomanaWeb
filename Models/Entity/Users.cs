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
        public string? Name { get; set; }
        public string? Phone { get; set; }      
        public string? Token { get; set; }        
        public string? Address { get; set; }    
        public string? FunctionPoint { get; set; }
        public string? Lat { get; set; }
        public string? Long { get; set; }
        public string? Password { get; set; }
        public bool? IsConfirm { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
        public string? Code { get; set; }
        /// <summary>When the current OTP becomes invalid (server time IQ).</summary>
        public DateTime? CodeExpiresAt { get; set; }
        /// <summary>Last successful OTP WhatsApp send (for cooldown / window).</summary>
        public DateTime? LastOtpSentAt { get; set; }
        /// <summary>Failed verify attempts for the current OTP; code invalidated after limit.</summary>
        public int OtpVerifyFailCount { get; set; } = 0;
        public int? CityId { get; set; }
        public int? NumberSendOtp { get; set; } = 0;
        public bool? IsBlock { get; set; }
    }
}
