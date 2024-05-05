 
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace RomanaWeb.Helper.Repository
{

    public class TwilioService : ITwilioService, IRegisterScopped
    {

         private DB_Context _Context;
         public TwilioService(DB_Context dB_Context) { _Context = dB_Context; }

       // public object TwilioClient { get; private set; }

        public async Task<ResObj> SendOTPCodeToPhoneNo(string PhoneNo,string OTPCode)
        {

            try
            {

                Users? person = await _Context.Users.FirstOrDefaultAsync(i => i.Phone == PhoneNo);
                if (person == null)
                    return Result.Return(false, "حسابك غير موجود حاول التسجيل مرة اخرى");

                var accountSid = "AC83d4b101c9aee06c8e114873385ad1e3";
                var authToken = "253c7d0e0fff0df3c5259ed729b1372d";
                TwilioClient.Init(accountSid, authToken);
                MessageResource _response = await MessageResource.CreateAsync
                (
                    body: $"رمز التحقق الخاص بك هو {OTPCode}",
                    from: new global::Twilio.Types.PhoneNumber($"+14242091843"),
                    // messagingServiceSid: "MG7a144626566d3ca69d4650645a46dab3" ,
                    to: new global::Twilio.Types.PhoneNumber($"+964{PhoneNo.Substring(1)}")
                );

                if (_response.Status == MessageResource.StatusEnum.Queued)
                {
                    person.IsConfirm = false;
                    person.Code = OTPCode;
                    _Context.Entry(person).State = EntityState.Modified;
                    await _Context.SaveChangesAsync();

                    return Result.Return(true, "تم ارسال كود التحقق الخاص بك");
                }
                else
                {
                    return Result.Return(false, "لم يتم ارسال كود التحقق الخاص بك حاول مرة اخرى");
                }
            }
            catch(Exception ex)
            {
                return Result.Return(false);
            }
        }
    }
}
