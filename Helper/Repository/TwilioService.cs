 
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


            Users? person = await _Context.Users.FirstOrDefaultAsync(i => i.Phone ==  PhoneNo);
            if (person == null)
                return Result.Return(false, "حسابك غير موجود حاول التسجيل مرة اخرى");


            //TwilioClient.Init("ACfd1cebb6be3402ceb483993d5a3b4053", "4ed325f9d3943e476e14201a1220c787");

            MessageResource _response = await MessageResource.CreateAsync
            (
                body: $"رمز التحقق الخاص بك هو {OTPCode}",
                from: new global::Twilio.Types.PhoneNumber($"+18573845268"),
                // messagingServiceSid: "MG7a144626566d3ca69d4650645a46dab3" ,
                to: new global::Twilio.Types.PhoneNumber($"+964{PhoneNo}")
            );

            if (_response.Status == MessageResource.StatusEnum.Queued)
            {


                person.IsConfirm = false;
                person.Code = OTPCode;
                _Context.Entry(person).State = EntityState.Modified;
                await _Context.SaveChangesAsync();

                return Result.Return(false, "تم ارسال كود التحقق الخاص بك");
            }
            else
            {
                return Result.Return(false, "لم يتم ارسال كود التحقق الخاص بك حاول مرة اخرى");
            }

        }
    }
}
