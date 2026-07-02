using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IOtpService
    {
        Task<ResObj> SendOTPCodeToPhoneNo(string PhoneNo, string OTPCode);
        /// <summary>Sends OTP via WhatsApp and stores code on the user record.</summary>
        Task<ResObj> SendOtpToUserAsync(Users user, string otpCode);
    }
}
