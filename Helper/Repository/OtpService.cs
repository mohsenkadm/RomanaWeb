using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RomanaWeb.Helper.Repository
{
    public class OtpService : IOtpService, IRegisterScopped
    {
        private readonly DB_Context _context;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.verifyway.com";
        private const string ApiKey = "1939$KINjztVxedf4Hes9wSeGsS9BrHxvAPeucU3V";

        public OtpService(DB_Context context)
        {
            _context = context;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ResObj> SendOTPCodeToPhoneNo(string PhoneNo, string OTPCode)
        {
            try
            {
                Users? person = await _context.Users.FirstOrDefaultAsync(i => i.Phone == PhoneNo && i.IsDelete != true);
                if (person == null)
                    return Result.Return(false, "حسابك غير موجود حاول التسجيل مرة اخرى");

                if (person.NumberSendOtp > 3)
                {
                    person.IsBlock = true;
                    _context.Entry(person).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Result.Return(false, "تم عمل بلوك لحسابك لارسالك لاكثر من 3 مرات يمكنك التواصل مع الادارة ");
                }

                var requestData = new
                {
                    recipient = $"964{PhoneNo.Substring(1)}",
                    type = "otp",
                    code = OTPCode,
                    channel = "whatsapp"
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/v1/", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObj = JsonSerializer.Deserialize<ApiResponse>(responseContent);

                    if (responseObj?.status == "success")
                    {
                        person.IsConfirm = false;
                        person.Code = OTPCode;
                        person.NumberSendOtp = (person.NumberSendOtp ?? 0) + 1;
                        _context.Entry(person).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        return Result.Return(true, "تم ارسال كود التحقق الخاص بك");
                    }
                    else
                    {
                        return Result.Return(false, "لم يتم ارسال كود التحقق الخاص بك حاول مرة اخرى");
                    }
                }
                else
                {
                    return Result.Return(false, "لم يتم ارسال كود التحقق الخاص بك حاول مرة اخرى");
                }
            }
            catch (Exception ex)
            {
                return Result.Return(false, "حدث خطأ ما أثناء الإرسال" + ex.ToString());
            }
        }

        private class ApiResponse
        {
            public string status { get; set; }
            public string message { get; set; }
        }
    }
}
