using RomanaWeb.Models.Entity;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using AutoMapper;

namespace RomanaWeb.Helper.Repository
{
    public class NotificationService    : MasterService, INotificationService,IRegisterScopped 
    {                                                          

        public NotificationService(
            DB_Context context,IMapper mapper) :base(mapper,context)
        {                            
        }

        public async Task<ResObj> GetNotificationAll(int? UserId)
        {
            List<Notification> data = await _Context.Notification.AsSplitQuery().AsNoTracking().Where(i => i.UserId == UserId || (i.ResId == 0 && i.UserId == 0)).ToListAsync();
            return Result.Return(true, data);
        }                    
        public async Task<ResObj> GetNotificationForRes(int? ResId)
        {
            List<Notification> data = await _Context.Notification.AsSplitQuery().AsNoTracking().Where(i => i.ResId == ResId).ToListAsync();
            return Result.Return(true, data);
        }
        public async Task<ResObj> Post(Notification notification)
        {
            notification.DateInsert = Key.DateTimeIQ;    
            await _Context.Notification.AddAsync(notification);
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح");
        }


    }
}
