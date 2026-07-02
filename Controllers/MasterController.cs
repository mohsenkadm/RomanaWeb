using RomanaWeb.Classes;
using RomanaWeb.Model.General;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http.Headers;

namespace RomanaWeb.Controllers
{  
      
    public class MasterController : ControllerBase
    {

        protected UserManager UserManager
        {
            get
            {                                                     
                // reading claim "UserProfile" from JWT Token
                string user = HttpContext.User.Claims
                    .Where(x => x.Type == ClaimInfo.UserManager)
                    .FirstOrDefault().Value;

                // if claim is exists then deserialize it 
                if (!string.IsNullOrWhiteSpace(user))
                {
                    return JsonConvert.DeserializeObject<UserManager>(user);
                }

                // if no claim is found return null means the user is not logged in
                return null;
            }
        }  
        [NonAction]
        public new IActionResult Response(bool success)
        {
            return Ok(new { success });
        }

        [NonAction]
        public new IActionResult Response(bool success, object data)
        {
            return Ok(new { success, data });
        }
         
        [NonAction]
        public new IActionResult Response(bool success, string msgCode)
        { 
                return Ok(new { success, msg = msgCode });
        }

        [NonAction]
        public new IActionResult Response(bool success, string msgCode, object data)
        { 
                return Ok(new { success, msg = msgCode, data });
        }

        [NonAction]
        public async Task OneSignalSender(string Title, string body)
        {
            try
            {
                string appId = "c8dfb903-9803-44d7-b6d5-b7660fa2e527";
                string apiKey = "ZGZlMjlmOGYtNWQ1My00ZjVlLWFjOTctYjY1M2MxYzkwOTc2";
                await SendOneSignalAsync(appId, apiKey, Title, body, null, true);
            }
            catch(Exception ex)
            {

            }
        }      
        [NonAction]
        public async Task OneSignalSenderUser(string Title, string body, List<string> id)
        {
            string appId = "c8dfb903-9803-44d7-b6d5-b7660fa2e527";
            string apiKey = "ZGZlMjlmOGYtNWQ1My00ZjVlLWFjOTctYjY1M2MxYzkwOTc2";
            await SendOneSignalAsync(appId, apiKey, Title, body, id, false);

        }

        // Sends a push to the USER app and includes a structured `data` payload that
        // the mobile client (Flutter / Android extension) reads to navigate.
        // Uses the OneSignal REST API directly so the JSON shape exactly matches the
        // contract agreed with the mobile team (type/orderId/status/statusText/...).
        [NonAction]
        public async Task OneSignalSenderUserWithOrder(
            string title,
            string body,
            List<string> ids,
            long orderId,
            string statusKey,
            string statusAr,
            string accentArgb,
            int? statusCode = null,
            string displayMode = "banner")
        {
            await SendOneSignalOrderStatusAsync(
                "c8dfb903-9803-44d7-b6d5-b7660fa2e527",
                "ZGZlMjlmOGYtNWQ1My00ZjVlLWFjOTctYjY1M2MxYzkwOTc2",
                title, body, ids, orderId, statusKey, statusAr, accentArgb, statusCode, displayMode);
        }

        [NonAction]
        public async Task OneSignalSenderResWithOrder(
            string title,
            string body,
            List<string> ids,
            long orderId,
            string statusKey,
            string statusAr,
            string accentArgb,
            int? statusCode = null,
            string displayMode = "banner")
        {
            await SendOneSignalOrderStatusAsync(
                "d3bfb798-5430-47c6-b921-38e1fddbcd26",
                "ZDVjYzVhNjAtYzc0Yy00Y2NmLWIwNjctMzBkNzNhOGU5MTk2",
                title, body, ids, orderId, statusKey, statusAr, accentArgb, statusCode, displayMode);
        }

        [NonAction]
        private static async Task SendOneSignalOrderStatusAsync(
            string onesignalAppId,
            string onesignalRestId,
            string title,
            string body,
            List<string> ids,
            long orderId,
            string statusKey,
            string statusAr,
            string accentArgb,
            int? statusCode,
            string displayMode)
        {
            try
            {
                var payload = new
                {
                    app_id = onesignalAppId,
                    target_channel = "push",
                    include_external_user_ids = ids,
                    channel_for_external_user_ids = "push",

                    headings = new { ar = title, en = title },
                    contents = new { ar = body, en = body },
                    subtitle = new { ar = body, en = body },

                    data = new
                    {
                        type = "order_status",
                        orderId = orderId.ToString(),
                        status = statusKey,
                        statusText = statusAr,
                        statusCode = statusCode?.ToString() ?? "",
                        displayMode
                    },

                    collapse_id = $"order_{orderId}",
                    priority = 10,
                    android_accent_color = accentArgb,
                    android_visibility = 1,
                    ttl = 86400,
                    small_icon = "ic_stat_onesignal_default",
                    large_icon = "https://mazyadmohammed-001-site1.anytempurl.com//Uplouds/IMG_2984.png"
                };

                using var http = new System.Net.Http.HttpClient();
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", onesignalRestId);
                http.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonConvert.SerializeObject(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await http.PostAsync("https://onesignal.com/api/v1/notifications", content);
                var respBody = await resp.Content.ReadAsStringAsync();
                Console.WriteLine($"OneSignal order_status push -> {resp.StatusCode}: {respBody}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendOneSignalOrderStatusAsync failed: " + ex.Message);
            }
        }



        [NonAction]
        public async Task OneSignalSenderResAll(string Title, string body)
        {
            try
            {
                string appId = "d3bfb798-5430-47c6-b921-38e1fddbcd26";
                string apiKey = "ZDVjYzVhNjAtYzc0Yy00Y2NmLWIwNjctMzBkNzNhOGU5MTk2";
                await SendOneSignalAsync(appId, apiKey, Title, body, null, true);
            }
            catch (Exception ex)
            {

            }
        }

        [NonAction]
        public async Task OneSignalSenderRes(string Title, string body, List<string> id)
        {
            string appId = "d3bfb798-5430-47c6-b921-38e1fddbcd26";
            string apiKey = "ZDVjYzVhNjAtYzc0Yy00Y2NmLWIwNjctMzBkNzNhOGU5MTk2";
            await SendOneSignalAsync(appId, apiKey, Title, body, id, false);

        }  

        [NonAction]
        public async Task OneSignalSenderSalAll(string Title, string body)
        {
            try
            {
                string appId = "5020facf-da15-4dec-8287-a8a0ee8ac0a6";
                string apiKey = "ODY1MWU3OTktMjA5ZC00YWRiLTlmYTYtM2I2NGI4YTg5ZmQy";
                await SendOneSignalAsync(appId, apiKey, Title, body, null, true);
            }
            catch (Exception ex)
            {

            }
        }

        [NonAction]
        public async Task OneSignalSenderSal(string Title, string body, List<string> id)
        {
            string appId = "5020facf-da15-4dec-8287-a8a0ee8ac0a6";
            string apiKey = "ODY1MWU3OTktMjA5ZC00YWRiLTlmYTYtM2I2NGI4YTg5ZmQy";
            await SendOneSignalAsync(appId, apiKey, Title, body, id, false);

        }

        [NonAction]
        private static async Task SendOneSignalAsync(
            string appId,
            string apiKey,
            string title,
            string body,
            List<string>? externalIds,
            bool broadcast)
        {
            var payload = new Dictionary<string, object?>
            {
                ["app_id"] = appId,
                ["headings"] = new { ar = title, en = title },
                ["contents"] = new { ar = body, en = body },
                ["subtitle"] = new { ar = body, en = body },
                ["large_icon"] = "https://mazyadmohammed-001-site1.anytempurl.com//Uplouds/IMG_2984.png"
            };

            if (broadcast)
                payload["included_segments"] = new[] { "Total Subscriptions" };
            else
            {
                payload["target_channel"] = "push";
                payload["include_external_user_ids"] = externalIds ?? new List<string>();
                payload["channel_for_external_user_ids"] = "push";
            }

            using var http = new System.Net.Http.HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKey);
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = JsonConvert.SerializeObject(payload);
            using var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
            _ = await http.PostAsync("https://onesignal.com/api/v1/notifications", content);
        }
    }
}
