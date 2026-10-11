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
            var query =
                from p in _Context.PromoCodes.AsNoTracking()
                join r in _Context.Restaurant.AsNoTracking()
                    on p.RestaurantId equals r.RestaurantId into rj
                from r in rj.DefaultIfEmpty()
                join a in _Context.Admin.AsNoTracking()
                    on p.CreatedByAdminId equals (int?)a.AdminId into aj
                from a in aj.DefaultIfEmpty()
                select new
                {
                    Promo = p,
                    RestaurantName = r != null ? r.Name : null,
                    AdminName = a != null ? a.AdminName : null
                };

            if (!string.IsNullOrWhiteSpace(Name))
            {
                var name = Name.Trim();
                query = query.Where(x => x.Promo.PromoName.Contains(name));
            }

            if (ResId is > 0)
                query = query.Where(x => x.Promo.RestaurantId == ResId || x.Promo.IsForAllStores);

            var rows = await query
                .OrderByDescending(x => x.Promo.CreatedAt)
                .ThenByDescending(x => x.Promo.PromoCodeId)
                .ToListAsync();

            var item = rows.Select(x =>
            {
                x.Promo.RestaurantName = x.RestaurantName;
                x.Promo.CreatedByAdminName = x.AdminName;
                return x.Promo;
            }).ToList();

            return Result.Return(true, item);
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
        private static string ResolveDiscountType(PromoCode promo)
        {
            if (!string.IsNullOrWhiteSpace(promo.DiscountType))
                return promo.DiscountType.Trim();
            if (promo.DiscountAmount > 0) return "Fixed";
            return "Percentage";
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

                PromoCode.DiscountType = ResolveDiscountType(PromoCode);
                PromoCode.IsActive = true;
                PromoCode.UsedOrders = 0;
                PromoCode.FirstUsedAt = null;
                if (PromoCode.CreatedAt == null)
                    PromoCode.CreatedAt = Key.DateTimeIQ;
                await _Context.PromoCodes.AddAsync(PromoCode);
            }
            else
            {
                var item = await _Context.PromoCodes.FirstOrDefaultAsync(i => i.PromoCodeId == PromoCode.PromoCodeId);
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
                    item.DiscountType = string.IsNullOrWhiteSpace(PromoCode.DiscountType)
                        ? ResolveDiscountType(PromoCode)
                        : PromoCode.DiscountType.Trim();
                    item.MaxDiscountAmount = PromoCode.MaxDiscountAmount;
                    item.MaxUsagePerUser = PromoCode.MaxUsagePerUser;
                    item.IsActive = PromoCode.IsActive;
                    // Preserve CreatedByAdminId / CreatedAt on update
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", PromoCode);
        }

        /// <summary>
        /// Pending = non-cancelled orders that hold this promo but have not yet committed usage
        /// (waiting for restaurant approval).
        /// </summary>
        private async Task<int> CountPendingPromoOrdersAsync(string promoName, int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(promoName))
                return 0;

            var name = promoName.Trim();
            var q = _Context.Orders.AsNoTracking()
                .Where(o => !o.IsCancel
                            && !o.IsPromoUsageCommitted
                            && o.PromoCode != null
                            && o.PromoCode == name);

            if (userId is > 0)
                q = q.Where(o => o.UserId == userId.Value);

            return await q.CountAsync();
        }

        private async Task<int> GetEffectiveUsedOrdersAsync(PromoCode promo)
        {
            int pending = await CountPendingPromoOrdersAsync(promo.PromoName);
            return promo.UsedOrders + pending;
        }

        private async Task<ResObj?> CheckPerUserLimitAsync(PromoCode promo, int userId)
        {
            if (userId <= 0 || promo.MaxUsagePerUser <= 0)
                return null;

            var usage = await _Context.PromoCodeUsage.AsNoTracking()
                .FirstOrDefaultAsync(u => u.PromoCodeId == promo.PromoCodeId && u.UserId == userId);

            int used = usage?.UsedCount ?? 0;
            int pending = await CountPendingPromoOrdersAsync(promo.PromoName, userId);
            if (used + pending >= promo.MaxUsagePerUser)
                return Result.Return(false, "لقد استخدمت هذا الكود الحد الأقصى المسموح لك");

            return null;
        }

        private async Task<ResObj?> CheckGlobalLimitAsync(PromoCode promo, bool persistDeactivate)
        {
            if (promo.MaxOrders <= 0)
                return null;

            int effectiveUsed = await GetEffectiveUsedOrdersAsync(promo);
            if (effectiveUsed >= promo.MaxOrders)
            {
                if (promo.UsedOrders >= promo.MaxOrders)
                {
                    if (persistDeactivate && promo.IsActive)
                    {
                        promo.IsActive = false;
                        _Context.Entry(promo).State = EntityState.Modified;
                        await _Context.SaveChangesAsync();
                    }
                }
                return Result.Return(false, "كود الخصم تجاوز الحد الاقصى للاستخدام");
            }

            return null;
        }

        public async Task<ResObj> ValidatePromoCode(string promoName, int restaurantId, int userId = 0)
        {
            var promo = await _Context.PromoCodes.AsSplitQuery().AsNoTracking()
                .FirstOrDefaultAsync(i => i.PromoName == promoName);

            if (promo == null)
                return Result.Return(false, "كود الخصم غير موجود");

            if (!promo.IsActive)
                return Result.Return(false, "كود الخصم غير فعال");

            var globalLimit = await CheckGlobalLimitAsync(promo, persistDeactivate: true);
            if (globalLimit != null) return globalLimit;

            var perUser = await CheckPerUserLimitAsync(promo, userId);
            if (perUser != null) return perUser;

            // Section 3.2: scope check.
            // First-time activation safety net: if enabled and the promo has never been used,
            // treat it as GLOBAL regardless of its stored scope.
            bool effectiveGlobal = promo.IsForAllStores
                                   || (FirstUseForceGlobal && promo.FirstUsedAt == null);

            if (!effectiveGlobal && promo.RestaurantId != restaurantId)
                return Result.Return(false, "كود الخصم غير صالح لهذا المطعم");

            return Result.Return(true, "كود الخصم صالح", promo);
        }

        /// <summary>
        /// Increments global + per-user usage counters. Called from Apply (live app
        /// does not send PromoCode on order create, so approval cannot commit usage).
        /// </summary>
        private async Task RecordPromoUsageCountersAsync(PromoCode promo, int userId)
        {
            bool isFirstUse = promo.FirstUsedAt == null;
            promo.UsedOrders += 1;
            if (isFirstUse)
                promo.FirstUsedAt = DateTime.UtcNow;
            if (promo.MaxOrders > 0 && promo.UsedOrders >= promo.MaxOrders)
                promo.IsActive = false;

            if (userId > 0 && promo.MaxUsagePerUser > 0)
            {
                var usage = await _Context.PromoCodeUsage
                    .FirstOrDefaultAsync(u => u.PromoCodeId == promo.PromoCodeId && u.UserId == userId);
                if (usage == null)
                {
                    usage = new PromoCodeUsage
                    {
                        PromoCodeId = promo.PromoCodeId,
                        UserId = userId,
                        UsedCount = 1
                    };
                    await _Context.PromoCodeUsage.AddAsync(usage);
                }
                else
                {
                    usage.UsedCount += 1;
                    _Context.Entry(usage).State = EntityState.Modified;
                }
            }

            _Context.Entry(promo).State = EntityState.Modified;
            await _Context.SaveChangesAsync();
        }

        public async Task<ResObj> ApplyPromoCode(string promoName, int restaurantId, decimal orderTotal, int userId = 0)
        {
            var promo = await _Context.PromoCodes
                .FirstOrDefaultAsync(i => i.PromoName == promoName);

            if (promo == null)
                return Result.Return(false, "كود الخصم غير موجود");

            if (!promo.IsActive)
                return Result.Return(false, "كود الخصم غير فعال");

            if (promo.MaxUsagePerUser > 0 && userId <= 0)
                return Result.Return(false, "يجب تسجيل الدخول لاستخدام كود الخصم");

            var globalLimit = await CheckGlobalLimitAsync(promo, persistDeactivate: true);
            if (globalLimit != null) return globalLimit;

            var perUserCheck = await CheckPerUserLimitAsync(promo, userId);
            if (perUserCheck != null) return perUserCheck;

            bool isFirstUse = promo.FirstUsedAt == null;
            bool effectiveGlobal = promo.IsForAllStores || (FirstUseForceGlobal && isFirstUse);

            if (!effectiveGlobal && promo.RestaurantId != restaurantId)
                return Result.Return(false, "كود الخصم غير صالح لهذا المطعم");

            decimal discountValue;
            if (string.Equals(promo.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
                discountValue = orderTotal * promo.Percentage / 100m;
            else if (string.Equals(promo.DiscountType, "Fixed", StringComparison.OrdinalIgnoreCase))
                discountValue = (decimal)promo.DiscountAmount;
            else if (promo.DiscountAmount > 0)
                discountValue = (decimal)promo.DiscountAmount;
            else
                discountValue = orderTotal * promo.Percentage / 100m;

            // Apply admin-defined ceiling.
            if (promo.MaxDiscountAmount > 0 && discountValue > promo.MaxDiscountAmount)
                discountValue = promo.MaxDiscountAmount;

            if (discountValue > orderTotal)
                discountValue = orderTotal;

            decimal netAmount = orderTotal - discountValue;

            // Live app omits PromoCode on order create — usage is recorded here.
            await RecordPromoUsageCountersAsync(promo, userId);

            // Section 3.2: store-scoped promo => discount is funded by the store, not the platform.
            // Global promo => funded by admin/platform.
            bool fundedByStore = !promo.IsForAllStores;

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

        /// <summary>
        /// Marks the order so Release can run on cancel. Counters are already
        /// incremented in ApplyPromoCode — do not increment again here.
        /// </summary>
        public async Task CommitPromoUsageForOrderAsync(Orders order)
        {
            if (order == null || order.IsPromoUsageCommitted)
                return;
            if (string.IsNullOrWhiteSpace(order.PromoCode))
                return;

            order.IsPromoUsageCommitted = true;
            _Context.Entry(order).State = EntityState.Modified;
            await _Context.SaveChangesAsync();
        }

        public async Task ReleasePromoUsageForOrderAsync(Orders order)
        {
            if (order == null || !order.IsPromoUsageCommitted)
                return;
            if (string.IsNullOrWhiteSpace(order.PromoCode))
            {
                order.IsPromoUsageCommitted = false;
                return;
            }

            var promoName = order.PromoCode.Trim();
            var promo = await _Context.PromoCodes
                .FirstOrDefaultAsync(i => i.PromoName == promoName);
            if (promo != null)
            {
                if (promo.UsedOrders > 0)
                    promo.UsedOrders -= 1;

                if (promo.MaxOrders > 0 && promo.UsedOrders < promo.MaxOrders && !promo.IsActive)
                    promo.IsActive = true;

                if (order.UserId > 0 && promo.MaxUsagePerUser > 0)
                {
                    var usage = await _Context.PromoCodeUsage
                        .FirstOrDefaultAsync(u => u.PromoCodeId == promo.PromoCodeId && u.UserId == order.UserId);
                    if (usage != null && usage.UsedCount > 0)
                    {
                        usage.UsedCount -= 1;
                        _Context.Entry(usage).State = EntityState.Modified;
                    }
                }

                _Context.Entry(promo).State = EntityState.Modified;
            }

            order.IsPromoUsageCommitted = false;
            _Context.Entry(order).State = EntityState.Modified;
            await _Context.SaveChangesAsync();
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
                decimal perUse = (decimal)(string.Equals(p.DiscountType, "Fixed", StringComparison.OrdinalIgnoreCase) || p.DiscountAmount > 0
                    ? p.DiscountAmount
                    : 0); // percentage liability needs order totals; reported separately by order pipeline
                if (p.MaxDiscountAmount > 0 && perUse > p.MaxDiscountAmount) perUse = p.MaxDiscountAmount;

                decimal estimatedTotal = (decimal)(perUse * p.UsedOrders);
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
