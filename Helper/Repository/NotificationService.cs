using RomanaWeb.Models.Entity;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using AutoMapper;
using System.Linq;

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
            List<Notification> data;
            if(UserId==0)
            data = await _Context.Notification.AsSplitQuery().AsNoTracking().Where(i => i.ResId == 0 && i.UserId == 0 && i.SaleManId==0).OrderByDescending(i=>i.NotificationId).ToListAsync();
            else  data = await _Context.Notification.AsSplitQuery().AsNoTracking().Where(i => i.UserId == UserId || (i.UserId==0 && i.ResId == 0 )).OrderByDescending(i=>i.NotificationId).ToListAsync();
            return Result.Return(true, data);
        }                    
        public async Task<ResObj> GetNotificationForRes(int? ResId)
        {
            List<Notification> data = await _Context.Notification.AsSplitQuery().AsNoTracking().Where(i => i.ResId == ResId).OrderByDescending(i => i.NotificationId).ToListAsync();
            return Result.Return(true, data);
        }                     
        public async Task<ResObj> GetNotificationForSale(int? SaleId)
        {
            List<Notification> data = await _Context.Notification.AsSplitQuery().AsNoTracking().Where(i => i.SaleManId == SaleId).OrderByDescending(i => i.NotificationId).ToListAsync();
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
