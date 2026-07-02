using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    /// <summary>
    /// Section 7 - Unified notification dispatcher (ported from Mazyad).
    /// OneSignal keys are read from configuration ("OneSignal:User|Restaurant|Driver").
    /// </summary>
    public class NotificationDispatcher : INotificationDispatcher, IRegisterScopped
    {
        private readonly DB_Context _context;
        private readonly ILoggerRepository _logger;
        private readonly IConfiguration _config;

        public NotificationDispatcher(DB_Context context, ILoggerRepository logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public Task SendAsync(NotificationAudience audience, int recipientId, NotificationPayload payload)
            => SendManyAsync(audience, new[] { recipientId }, payload);

        public async Task SendManyAsync(NotificationAudience audience, IEnumerable<int> recipientIds, NotificationPayload payload)
        {
            var ids = recipientIds?.Where(i => i > 0).Distinct().Select(i => i.ToString()).ToList()
                      ?? new List<string>();
            await PersistInbox(audience, ids, payload);
            if (ids.Count == 0) return;
            await PushExternal(audience, ids, payload, broadcast: false);
        }

        public async Task BroadcastAsync(NotificationAudience audience, NotificationPayload payload)
        {
            await PersistInbox(audience, recipientIds: null, payload);
            await PushExternal(audience, externalIds: null, payload, broadcast: true);
        }

        // -------- internals --------

        private async Task PersistInbox(NotificationAudience audience, List<string>? recipientIds, NotificationPayload payload)
        {
            try
            {
                if (recipientIds == null || recipientIds.Count == 0)
                {
                    // Broadcast => single inbox row addressed to everyone of that audience.
                    await _context.Notification.AddAsync(NewRow(audience, 0, payload));
                }
                else
                {
                    foreach (var idStr in recipientIds)
                    {
                        if (int.TryParse(idStr, out int id))
                            await _context.Notification.AddAsync(NewRow(audience, id, payload));
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "NotificationDispatcher => PersistInbox");
            }
        }

        private static RomanaWeb.Models.Entity.Notification NewRow(NotificationAudience audience, int recipientId, NotificationPayload payload)
        {
            var row = new RomanaWeb.Models.Entity.Notification
            {
                Title = payload.Title ?? "",
                Details = payload.Body ?? "",
                DateInsert = Key.DateTimeIQ,
                UserId = 0,
                ResId = 0,
                SaleManId = 0
            };
            switch (audience)
            {
                case NotificationAudience.User: row.UserId = recipientId; break;
                case NotificationAudience.Restaurant: row.ResId = recipientId; break;
                case NotificationAudience.Driver: row.SaleManId = recipientId; break;
            }
            return row;
        }

        private async Task PushExternal(NotificationAudience audience, List<string>? externalIds, NotificationPayload payload, bool broadcast)
        {
            try
            {
                string section = audience switch
                {
                    NotificationAudience.Restaurant => "OneSignal:Restaurant",
                    NotificationAudience.Driver => "OneSignal:Driver",
                    _ => "OneSignal:User"
                };
                string appId = _config[$"{section}:AppId"] ?? "";
                string apiKey = _config[$"{section}:ApiKey"] ?? "";
                if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(apiKey))
                {
                    await _logger.WriteAsync(new Exception($"Missing OneSignal config for {section}"), "NotificationDispatcher => PushExternal");
                    return;
                }

                var pushPayload = new Dictionary<string, object?>
                {
                    ["app_id"] = appId,
                    ["headings"] = new { ar = payload.Title ?? "", en = payload.Title ?? "" },
                    ["contents"] = new { ar = payload.Body ?? "", en = payload.Body ?? "" },
                    ["subtitle"] = new { ar = payload.Body ?? "", en = payload.Body ?? "" },
                    ["large_icon"] = "https://mazyadmohammed-001-site1.anytempurl.com//Uplouds/IMG_2984.png",
                    ["priority"] = 10,
                    ["android_visibility"] = 1,
                    ["ttl"] = 86400,
                    ["small_icon"] = "ic_stat_onesignal_default"
                };

                if (payload.Data != null && payload.Data.Count > 0)
                    pushPayload["data"] = payload.Data;

                if (broadcast)
                    pushPayload["included_segments"] = new[] { "Total Subscriptions" };
                else
                {
                    pushPayload["target_channel"] = "push";
                    pushPayload["include_external_user_ids"] = externalIds ?? new List<string>();
                    pushPayload["channel_for_external_user_ids"] = "push";
                }

                using var http = new HttpClient();
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKey);
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = JsonConvert.SerializeObject(pushPayload);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                using var response = await http.PostAsync("https://onesignal.com/api/v1/notifications", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await _logger.WriteAsync(
                        new Exception($"OneSignal push failed ({response.StatusCode}) for {section}: {responseBody}"),
                        "NotificationDispatcher => PushExternal");
                }
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "NotificationDispatcher => PushExternal");
            }
        }
    }
}
