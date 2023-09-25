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
using OneSignalApi.Api;
using OneSignalApi.Client;
using OneSignalApi.Model;
using Configuration = OneSignalApi.Client.Configuration;

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
                string onesignalAppID = "c8dfb903-9803-44d7-b6d5-b7660fa2e527";
                string onesignalRestID = "ZGZlMjlmOGYtNWQ1My00ZjVlLWFjOTctYjY1M2MxYzkwOTc2";


                // Configure the OneSignal Library
                var appConfig = new Configuration();
                appConfig.BasePath = "https://onesignal.com/api/v1";
                appConfig.AccessToken = onesignalRestID;
                var appInstance = new DefaultApi(appConfig);

                // Create and send notification to all subscribed users
                var notification = new Notification(appId: onesignalAppID)
                {
                    Contents = new StringMap(body), Headings=new StringMap(Title),Subtitle=new StringMap(body),
                    IncludedSegments = new List<string> { "Subscribed Users" },
                };
                var response = await appInstance.CreateNotificationAsync(notification);

                Console.WriteLine($"Notification created for {response.Recipients} recipients");
            }
            catch(Exception ex)
            {

            }
        }      
        [NonAction]
        public async Task OneSignalSenderUser(string Title, string body, List<string> id)
        {
            string onesignalAppID = "c8dfb903-9803-44d7-b6d5-b7660fa2e527";
            string onesignalRestID = "ZGZlMjlmOGYtNWQ1My00ZjVlLWFjOTctYjY1M2MxYzkwOTc2";


            // Configure the OneSignal Library
            var appConfig = new Configuration();
            appConfig.BasePath = "https://onesignal.com/api/v1";
            appConfig.AccessToken = onesignalRestID;
            var appInstance = new DefaultApi(appConfig);

            // Create and send notification to all subscribed users
            var notification = new Notification(appId: onesignalAppID)
            {
                Contents = new StringMap(body),
                Headings = new StringMap(Title),
                Subtitle = new StringMap(body),
                IncludeExternalUserIds = id ,
                   ChannelForExternalUserIds = "push"
            };
            var response = await appInstance.CreateNotificationAsync(notification);

            Console.WriteLine($"Notification created for {response.Recipients} recipients");
                     
        }
    }
}
