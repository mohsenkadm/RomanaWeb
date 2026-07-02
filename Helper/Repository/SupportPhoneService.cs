using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class SupportPhoneService : ISupportPhoneService, IRegisterScopped
    {
        private readonly DB_Context _context;

        public SupportPhoneService(DB_Context context)
        {
            _context = context;
        }

        public async Task<ResObj> GetAll()
        {
            var items = await _context.SupportPhone.AsNoTracking()
                .OrderBy(x => x.AppType)
                .ToListAsync();
            return Result.Return(true, items);
        }

        public async Task<ResObj> GetById(int id)
        {
            var item = await _context.SupportPhone.AsNoTracking()
                .FirstOrDefaultAsync(x => x.SupportPhoneId == id);
            if (item == null)
                return Result.Return(false, "السجل غير موجود");
            return Result.Return(true, item);
        }

        public async Task<ResObj> GetForApp(int appType)
        {
            if (!IsValidAppType(appType))
                return Result.Return(false, "نوع التطبيق غير صالح");

            var item = await _context.SupportPhone.AsNoTracking()
                .FirstOrDefaultAsync(x => x.AppType == appType && x.IsActive);
            if (item == null)
                return Result.Return(true, new { appType, phone = (string?)null, label = (string?)null });
            return Result.Return(true, new { appType = item.AppType, phone = item.Phone, label = item.Label });
        }

        public async Task<ResObj> GetAllForApps()
        {
            var items = await _context.SupportPhone.AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync();

            return Result.Return(true, new
            {
                user = items.FirstOrDefault(x => x.AppType == 1)?.Phone,
                restaurant = items.FirstOrDefault(x => x.AppType == 2)?.Phone,
                driver = items.FirstOrDefault(x => x.AppType == 3)?.Phone,
                items = items.Select(x => new
                {
                    x.AppType,
                    x.Phone,
                    x.Label
                })
            });
        }

        public async Task<ResObj> Post(SupportPhone supportPhone)
        {
            if (!IsValidAppType(supportPhone.AppType))
                return Result.Return(false, "نوع التطبيق غير صالح");
            if (string.IsNullOrWhiteSpace(supportPhone.Phone))
                return Result.Return(false, "رجاءا ادخل رقم الهاتف");

            supportPhone.Phone = supportPhone.Phone.Trim();

            var duplicate = await _context.SupportPhone
                .FirstOrDefaultAsync(x => x.AppType == supportPhone.AppType
                    && x.SupportPhoneId != supportPhone.SupportPhoneId);
            if (duplicate != null)
                return Result.Return(false, "يوجد رقم دعم مسجل مسبقا لهذا التطبيق");

            if (supportPhone.SupportPhoneId == 0)
            {
                await _context.SupportPhone.AddAsync(supportPhone);
            }
            else
            {
                var existing = await _context.SupportPhone
                    .FirstOrDefaultAsync(x => x.SupportPhoneId == supportPhone.SupportPhoneId);
                if (existing == null)
                    return Result.Return(false, "السجل غير موجود");

                existing.AppType = supportPhone.AppType;
                existing.Phone = supportPhone.Phone;
                existing.Label = supportPhone.Label;
                existing.IsActive = supportPhone.IsActive;
                _context.Entry(existing).State = EntityState.Modified;
                supportPhone = existing;
            }

            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", supportPhone);
        }

        public async Task<ResObj> Delete(int id)
        {
            var item = await _context.SupportPhone.FirstOrDefaultAsync(x => x.SupportPhoneId == id);
            if (item == null)
                return Result.Return(false, "السجل غير موجود");

            _context.SupportPhone.Remove(item);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحذف بنجاح");
        }

        private static bool IsValidAppType(int appType) => appType is >= 1 and <= 3;
    }
}
