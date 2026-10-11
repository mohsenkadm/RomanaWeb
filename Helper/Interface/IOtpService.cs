using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IOtpService
    {
        Task<ResObj> SendOTPCodeToPhoneNo(string PhoneNo, string OTPCode);
        /// <summary>
        /// Sends OTP via WhatsApp and stores code on the user record.
        /// When <paramref name="recipientPhone"/> is set, WhatsApp goes to that number
        /// (e.g. pending phone change) while OTP fields remain on <paramref name="user"/>.
        /// </summary>
        Task<ResObj> SendOtpToUserAsync(Users user, string otpCode, string? recipientPhone = null);
    }
}
