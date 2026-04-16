
using RomanaWeb.Classes;

namespace RomanaWeb.Helper.Interface
{
    public interface IOtpService
    {                
         Task<ResObj> SendOTPCodeToPhoneNo(string PhoneNo,string OTPCode);
    }
}
