using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;
namespace RomanaWeb.Helper.Repository
{
    public class PromoCodeService : MasterService, IPromoCodeService, IRegisterScopped
    {                                                                      
        public PromoCodeService(DB_Context dB_Context, IMapper mapper) : base(mapper, dB_Context)
        {                                       
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.PromoCodeId == Id);
            if (item != null)
            {
                _Context.PromoCodes.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }
            return Result.Return(false);
        }
        public async Task<ResObj> GetAll()
        {

            var item = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().ToListAsync();
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> GetById(int Id)
        {
            var item = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.PromoCodeId == Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> Post(PromoCode PromoCode)
        {
            if (PromoCode.PromoCodeId == 0)
            {
                var check = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.PromoName == PromoCode.PromoName);
                if (check != null)
                {
                    return Result.Return(false, "تم الحفظ سابقا");
                }

                await _Context.PromoCodes.AddAsync(PromoCode);
            }
            else
            {
                var item = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.PromoCodeId == PromoCode.PromoCodeId);
                if (item != null)
                {
                    item.Percentage = PromoCode.Percentage;
                    item.PromoName = PromoCode.PromoName;
                    _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", PromoCode);
        }
    }
}
