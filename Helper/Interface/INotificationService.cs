using RomanaWeb.Models.Entity;
using RomanaWeb.Classes;

namespace RomanaWeb.Helper.Interface
{
    public interface INotificationService
    {
        Task<ResObj> GetNotificationAll(int? Id);
        Task<ResObj> GetNotificationForRes(int? Id);
        Task<ResObj> Post(Notification notification);
    }
}
