
using RomanaWeb.Classes;

namespace RomanaWeb.Helper.Interface
{
    public interface ITwilioService
    {                
         Task<ResObj> SendOTPCodeToPhoneNo(string PhoneNo,string OTPCode);
    }
}
