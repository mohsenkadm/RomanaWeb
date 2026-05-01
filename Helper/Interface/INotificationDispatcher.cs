using OneSignalApi.Api;
using OneSignalApi.Client;
using OneSignalApi.Model;
using RomanaWeb.Classes;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public enum NotificationAudience
    {
        /// <summary>End-customer (Customer app).</summary>
        User,
        /// <summary>Merchant / restaurant (Merchant app).</summary>
        Restaurant,
        /// <summary>Driver / SaleMan (Driver app).</summary>
        Driver
    }

    public class NotificationPayload
    {
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        /// <summary>Optional structured data (deep-link, order id, etc.).</summary>
        public IDictionary<string, string>? Data { get; set; }
    }

    /// <summary>
    /// Section 7 - Unified notification sending mechanism.
    /// Ports the FCM/APNS pipeline pattern from the Mazyad project: a single facade
    /// that fans out to the correct OneSignal app per audience, and persists an
    /// in-app Notification row so it shows in the user's inbox.
    /// </summary>
    public interface INotificationDispatcher
    {
        /// <summary>Send to a single recipient (Customer app user, merchant, or driver).</summary>
        Task SendAsync(NotificationAudience audience, int recipientId, NotificationPayload payload);

        /// <summary>Broadcast to every subscribed recipient of an audience.</summary>
        Task BroadcastAsync(NotificationAudience audience, NotificationPayload payload);

        /// <summary>Send to many recipient ids of the same audience.</summary>
        Task SendManyAsync(NotificationAudience audience, IEnumerable<int> recipientIds, NotificationPayload payload);
    }
}
