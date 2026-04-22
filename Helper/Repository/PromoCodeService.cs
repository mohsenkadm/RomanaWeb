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
        private readonly IDapperRepository<PromoCode> _PromoCodeService;
        public PromoCodeService(DB_Context dB_Context, IMapper mapper,IDapperRepository<PromoCode> dapper) : base(mapper, dB_Context)
        {                                       
            _PromoCodeService = dapper;
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
        public async Task<ResObj> GetAll(string? Name, int? ResId)
        {

            var item = await _PromoCodeService.GetEntityListAsync("dbo.GetPromoCodesAll", new { Name , ResId });
            //var item = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().ToListAsync();
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
        public async Task<ResObj> GetByResId(int Id)
        {
            var item = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().Where(i=>i.RestaurantId==Id).ToListAsync();
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
                PromoCode.IsActive = true;
                PromoCode.UsedOrders = 0;
                await _Context.PromoCodes.AddAsync(PromoCode);
            }
            else
            {
                var item = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.PromoCodeId == PromoCode.PromoCodeId);
                if (item != null)
                {
                    item.Percentage = PromoCode.Percentage;
                    item.PromoName = PromoCode.PromoName;
                    item.RestaurantId = PromoCode.RestaurantId;
                    item.MaxOrders = PromoCode.MaxOrders;
                    item.IsForAllStores = PromoCode.IsForAllStores;
                    item.DiscountAmount = PromoCode.DiscountAmount;
                    item.IsActive = PromoCode.IsActive;
                    _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", PromoCode);
        }

        public async Task<ResObj> ValidatePromoCode(string promoName, int restaurantId)
        {
            var promo = await _Context.PromoCodes.AsSplitQuery().AsNoTracking()
                .FirstOrDefaultAsync(i => i.PromoName == promoName &&
                    (i.IsForAllStores || i.RestaurantId == restaurantId));

            if (promo == null)
                return Result.Return(false, "كود الخصم غير موجود");

            if (!promo.IsActive)
                return Result.Return(false, "كود الخصم غير فعال");

            if (promo.MaxOrders > 0 && promo.UsedOrders >= promo.MaxOrders)
            {
                promo.IsActive = false;
                _Context.Entry(promo).State = EntityState.Modified;
                await _Context.SaveChangesAsync();
                return Result.Return(false, "كود الخصم تجاوز الحد الاقصى للاستخدام");
            }

            return Result.Return(true, "كود الخصم صالح", promo);
        }

        public async Task<ResObj> ApplyPromoCode(string promoName, int restaurantId, decimal orderTotal)
        {
            var promo = await _Context.PromoCodes
                .FirstOrDefaultAsync(i => i.PromoName == promoName &&
                    (i.IsForAllStores || i.RestaurantId == restaurantId));

            if (promo == null)
                return Result.Return(false, "كود الخصم غير موجود");

            if (!promo.IsActive)
                return Result.Return(false, "كود الخصم غير فعال");

            if (promo.MaxOrders > 0 && promo.UsedOrders >= promo.MaxOrders)
            {
                promo.IsActive = false;
                _Context.Entry(promo).State = EntityState.Modified;
                await _Context.SaveChangesAsync();
                return Result.Return(false, "كود الخصم تجاوز الحد الاقصى للاستخدام");
            }

            // Calculate discount
            decimal discountValue = 0;
            if (promo.DiscountAmount > 0)
            {
                // Fixed monetary discount
                discountValue = promo.DiscountAmount;
            }
            else if (promo.Percentage > 0)
            {
                // Percentage discount
                discountValue = orderTotal * promo.Percentage / 100;
            }

            if (discountValue > orderTotal)
                discountValue = orderTotal;

            decimal netAmount = orderTotal - discountValue;

            // Increment usage
            promo.UsedOrders += 1;
            if (promo.MaxOrders > 0 && promo.UsedOrders >= promo.MaxOrders)
                promo.IsActive = false;

            _Context.Entry(promo).State = EntityState.Modified;
            await _Context.SaveChangesAsync();

            var result = new
            {
                PromoName = promo.PromoName,
                DiscountValue = Math.Round(discountValue, 0),
                NetAmount = Math.Round(netAmount, 0),
                OriginalTotal = orderTotal,
                UsedOrders = promo.UsedOrders,
                MaxOrders = promo.MaxOrders,
                IsActive = promo.IsActive
            };

            return Result.Return(true, "تم تطبيق كود الخصم بنجاح", result);
        }

        public async Task<ResObj> GetAnalytics(DateTime dateFrom, DateTime dateTo)
        {
            var promos = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().ToListAsync();

            var analytics = promos.Select(p => new
            {
                p.PromoCodeId,
                p.PromoName,
                p.RestaurantName,
                p.Percentage,
                p.DiscountAmount,
                p.MaxOrders,
                p.UsedOrders,
                p.IsActive,
                p.IsForAllStores,
                RemainingOrders = p.MaxOrders > 0 ? p.MaxOrders - p.UsedOrders : -1,
                UsagePercentage = p.MaxOrders > 0 ? Math.Round((double)p.UsedOrders / p.MaxOrders * 100, 1) : 0
            }).ToList();

            return Result.Return(true, analytics);
        }
    }
}
