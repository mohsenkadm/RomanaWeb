using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;
namespace RomanaWeb.Helper.Repository
{
    public class PromoCodeService : MasterService, IPromoCodeService, IRegisterScopped
    {
        private readonly IDapperRepository<PromoCode> _PromoCodeService;
        private readonly IConfiguration _config;
        public PromoCodeService(DB_Context dB_Context, IMapper mapper, IDapperRepository<PromoCode> dapper, IConfiguration config) : base(mapper, dB_Context)
        {
            _PromoCodeService = dapper;
            _config = config;
        }

        // Feature flag (Section 3.2): on first-time activation force-treat the promo as GLOBAL.
        private bool FirstUseForceGlobal => _config.GetValue<bool>("PromoCodes:FirstUseForceGlobal", true);
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

                // Section 3.1: STORE-scoped promo codes must reference a store.
                if (!PromoCode.IsForAllStores && PromoCode.RestaurantId == 0)
                    return Result.Return(false, "يجب اختيار المطعم لهذا النوع من البرومو كود");

                PromoCode.IsActive = true;
                PromoCode.UsedOrders = 0;
                PromoCode.FirstUsedAt = null;
                await _Context.PromoCodes.AddAsync(PromoCode);
            }
            else
            {
                var item = await _Context.PromoCodes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.PromoCodeId == PromoCode.PromoCodeId);
                if (item != null)
                {
                    if (!PromoCode.IsForAllStores && PromoCode.RestaurantId == 0)
                        return Result.Return(false, "يجب اختيار المطعم لهذا النوع من البرومو كود");

                    item.Percentage = PromoCode.Percentage;
                    item.PromoName = PromoCode.PromoName;
                    item.RestaurantId = PromoCode.RestaurantId;
                    item.MaxOrders = PromoCode.MaxOrders;
                    item.IsForAllStores = PromoCode.IsForAllStores;
                    item.DiscountAmount = PromoCode.DiscountAmount;
                    item.DiscountType = PromoCode.DiscountType;
                    item.MaxDiscountAmount = PromoCode.MaxDiscountAmount;
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
                .FirstOrDefaultAsync(i => i.PromoName == promoName);

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

            // Section 3.2: scope check.
            // First-time activation safety net: if enabled and the promo has never been used,
            // treat it as GLOBAL regardless of its stored scope.
            bool effectiveGlobal = promo.IsForAllStores
                                   || (FirstUseForceGlobal && promo.FirstUsedAt == null);

            if (!effectiveGlobal && promo.RestaurantId != restaurantId)
                return Result.Return(false, "كود الخصم غير صالح لهذا المطعم");

            return Result.Return(true, "كود الخصم صالح", promo);
        }

        public async Task<ResObj> ApplyPromoCode(string promoName, int restaurantId, decimal orderTotal)
        {
            var promo = await _Context.PromoCodes
                .FirstOrDefaultAsync(i => i.PromoName == promoName);

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

            bool isFirstUse = promo.FirstUsedAt == null;
            bool effectiveGlobal = promo.IsForAllStores || (FirstUseForceGlobal && isFirstUse);

            if (!effectiveGlobal && promo.RestaurantId != restaurantId)
                return Result.Return(false, "كود الخصم غير صالح لهذا المطعم");

            // Calculate discount.
            decimal discountValue;
            if (string.Equals(promo.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
                discountValue = orderTotal * promo.Percentage / 100m;
            else if (string.Equals(promo.DiscountType, "Fixed", StringComparison.OrdinalIgnoreCase))
                discountValue = promo.DiscountAmount;
            else if (promo.DiscountAmount > 0)
                discountValue = promo.DiscountAmount;
            else
                discountValue = orderTotal * promo.Percentage / 100m;

            // Apply admin-defined ceiling.
            if (promo.MaxDiscountAmount > 0 && discountValue > promo.MaxDiscountAmount)
                discountValue = promo.MaxDiscountAmount;

            if (discountValue > orderTotal)
                discountValue = orderTotal;

            decimal netAmount = orderTotal - discountValue;

            // Section 3.2: store-scoped promo => discount is funded by the store, not the platform.
            // Global promo => funded by admin/platform.
            bool fundedByStore = !promo.IsForAllStores;

            // Increment usage and stamp first-use.
            promo.UsedOrders += 1;
            if (isFirstUse) promo.FirstUsedAt = DateTime.UtcNow;
            if (promo.MaxOrders > 0 && promo.UsedOrders >= promo.MaxOrders)
                promo.IsActive = false;

            _Context.Entry(promo).State = EntityState.Modified;
            await _Context.SaveChangesAsync();

            var result = new
            {
                PromoName = promo.PromoName,
                Scope = promo.Scope,
                DiscountType = promo.DiscountType ?? (promo.DiscountAmount > 0 ? "Fixed" : "Percentage"),
                DiscountValue = Math.Round(discountValue, 0),
                MaxDiscountAmount = promo.MaxDiscountAmount,
                NetAmount = Math.Round(netAmount, 0),
                OriginalTotal = orderTotal,
                UsedOrders = promo.UsedOrders,
                MaxOrders = promo.MaxOrders,
                IsActive = promo.IsActive,
                FundedByStore = fundedByStore,
                FundedByPlatform = !fundedByStore,
                FirstTimeActivation = isFirstUse,
                EffectiveScope = effectiveGlobal ? "GLOBAL" : "STORE"
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
                p.DiscountType,
                p.MaxDiscountAmount,
                p.MaxOrders,
                p.UsedOrders,
                p.IsActive,
                p.IsForAllStores,
                Scope = p.Scope,
                FundedByStore = p.FundedByStore,
                p.FirstUsedAt,
                RemainingOrders = p.MaxOrders > 0 ? p.MaxOrders - p.UsedOrders : -1,
                UsagePercentage = p.MaxOrders > 0 ? Math.Round((double)p.UsedOrders / p.MaxOrders * 100, 1) : 0
            }).ToList();

            return Result.Return(true, analytics);
        }

        // Section 3.3: report on amounts owed/deducted from store payouts due to promo discounts.
        // NOTE: This currently aggregates expected liability per active promo
        // (UsedOrders * resolved discount value, capped by MaxDiscountAmount).
        // For a per-order ledger, an OrderPromoUsage table should be added.
        public async Task<ResObj> GetStoreLiabilityReport(int? restaurantId, DateTime dateFrom, DateTime dateTo)
        {
            var query = _Context.PromoCodes.AsSplitQuery().AsNoTracking().AsQueryable();
            if (restaurantId.HasValue && restaurantId.Value > 0)
                query = query.Where(p => p.RestaurantId == restaurantId.Value);

            var promos = await query.ToListAsync();

            var report = promos.Select(p =>
            {
                decimal perUse = string.Equals(p.DiscountType, "Fixed", StringComparison.OrdinalIgnoreCase) || p.DiscountAmount > 0
                    ? p.DiscountAmount
                    : 0; // percentage liability needs order totals; reported separately by order pipeline
                if (p.MaxDiscountAmount > 0 && perUse > p.MaxDiscountAmount) perUse = p.MaxDiscountAmount;

                decimal estimatedTotal = perUse * p.UsedOrders;
                bool fundedByStore = !p.IsForAllStores;

                return new
                {
                    p.PromoCodeId,
                    p.PromoName,
                    p.RestaurantId,
                    p.RestaurantName,
                    Scope = p.Scope,
                    FundedByStore = fundedByStore,
                    FundedByPlatform = !fundedByStore,
                    p.UsedOrders,
                    EstimatedDiscountPerUse = perUse,
                    EstimatedTotalDiscount = estimatedTotal,
                    StoreOwes = fundedByStore ? estimatedTotal : 0m,
                    PlatformOwes = fundedByStore ? 0m : estimatedTotal
                };
            }).ToList();

            var summary = new
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                TotalStoreFunded = report.Sum(r => r.StoreOwes),
                TotalPlatformFunded = report.Sum(r => r.PlatformOwes),
                Items = report
            };

            return Result.Return(true, summary);
        }
    }
}
