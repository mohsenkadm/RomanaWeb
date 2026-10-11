using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class SaleManBalanceService : ISaleManBalanceService, IRegisterScopped
    {
        private readonly DB_Context _context;

        public SaleManBalanceService(DB_Context context)
        {
            _context = context;
        }

        /// <summary>Food net = NetAmount when set, else Total. Never includes CostDelivery.</summary>
        public static decimal ResolveFoodNet(Orders order)
        {
            if (order.NetAmount > 0)
                return (decimal)order.NetAmount;
            if (order.Total > 0)
                return (decimal)order.Total;
            return 0m;
        }

        public async Task AccrueOnDeliveredAsync(Orders order)
        {
            if (order == null || order.OrderId <= 0)
                return;
            if (order.SaleManId is null or <= 0)
                return;
            if (order.IsCancel)
                return;

            bool delivered = order.IsDelivered == true || order.IsDeliveryConfirmed;
            if (!delivered)
                return;

            bool exists = await _context.SaleManBalanceEntry
                .AnyAsync(e => e.OrderId == order.OrderId);
            if (exists)
                return;

            int saleManId = order.SaleManId.Value;
            decimal foodNet = ResolveFoodNet(order);
            decimal deliveryFee = order.CostDelivery ?? 0m;
            if (foodNet < 0) foodNet = 0;
            if (deliveryFee < 0) deliveryFee = 0;

            var entry = new SaleManBalanceEntry
            {
                SaleManId = saleManId,
                OrderId = order.OrderId,
                FoodNetAmount = foodNet,
                DeliveryFeeAmount = deliveryFee,
                CreatedAt = Key.DateTimeIQ,
                SettlementId = null
            };
            await _context.SaleManBalanceEntry.AddAsync(entry);

            var balance = await _context.SaleManBalance
                .FirstOrDefaultAsync(b => b.SaleManId == saleManId);
            if (balance == null)
            {
                balance = new SaleManBalance
                {
                    SaleManId = saleManId,
                    FoodNetBalance = foodNet,
                    DeliveryFeeBalance = deliveryFee,
                    UpdatedAt = Key.DateTimeIQ
                };
                await _context.SaleManBalance.AddAsync(balance);
            }
            else
            {
                balance.FoodNetBalance += foodNet;
                balance.DeliveryFeeBalance += deliveryFee;
                balance.UpdatedAt = Key.DateTimeIQ;
                _context.Entry(balance).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ResObj> GetBalanceAsync(int saleManId)
        {
            var driver = await _context.SaleMan.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SaleManId == saleManId);
            if (driver == null || driver.IsDelete == true)
                return Result.Return(false, "المندوب غير موجود");

            var balance = await _context.SaleManBalance.AsNoTracking()
                .FirstOrDefaultAsync(b => b.SaleManId == saleManId);

            int openOrders = await _context.SaleManBalanceEntry.AsNoTracking()
                .CountAsync(e => e.SaleManId == saleManId && e.SettlementId == null);

            return Result.Return(true, new
            {
                saleManId,
                name = driver.Name,
                phone = driver.Phone,
                foodNetBalance = balance?.FoodNetBalance ?? 0m,
                deliveryFeeBalance = balance?.DeliveryFeeBalance ?? 0m,
                openOrdersCount = openOrders,
                updatedAt = balance?.UpdatedAt
            });
        }

        public async Task<Dictionary<int, SaleManBalance>> GetBalancesMapAsync(IEnumerable<int> saleManIds)
        {
            var ids = saleManIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new Dictionary<int, SaleManBalance>();

            var list = await _context.SaleManBalance.AsNoTracking()
                .Where(b => ids.Contains(b.SaleManId))
                .ToListAsync();
            return list.ToDictionary(b => b.SaleManId);
        }

        public async Task<ResObj> SettleAsync(int saleManId, int? adminId, string? note)
        {
            var driver = await _context.SaleMan
                .FirstOrDefaultAsync(s => s.SaleManId == saleManId);
            if (driver == null || driver.IsDelete == true)
                return Result.Return(false, "المندوب غير موجود");

            var balance = await _context.SaleManBalance
                .FirstOrDefaultAsync(b => b.SaleManId == saleManId);

            decimal food = balance?.FoodNetBalance ?? 0m;
            decimal fees = balance?.DeliveryFeeBalance ?? 0m;

            if (food <= 0 && fees <= 0)
            {
                bool hasOpen = await _context.SaleManBalanceEntry
                    .AnyAsync(e => e.SaleManId == saleManId && e.SettlementId == null);
                if (!hasOpen)
                    return Result.Return(false, "لا يوجد رصيد للتصفير — الحساب صفري حالياً");
            }

            var settlement = new SaleManSettlement
            {
                SaleManId = saleManId,
                FoodNetAmount = food,
                DeliveryFeeAmount = fees,
                SettledAt = Key.DateTimeIQ,
                SettledByAdminId = adminId,
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
            };
            if (settlement.Note != null && settlement.Note.Length > 500)
                settlement.Note = settlement.Note.Substring(0, 500);

            await _context.SaleManSettlement.AddAsync(settlement);
            await _context.SaveChangesAsync();

            var openEntries = await _context.SaleManBalanceEntry
                .Where(e => e.SaleManId == saleManId && e.SettlementId == null)
                .ToListAsync();
            foreach (var e in openEntries)
            {
                e.SettlementId = settlement.SettlementId;
                _context.Entry(e).State = EntityState.Modified;
            }

            if (balance == null)
            {
                balance = new SaleManBalance
                {
                    SaleManId = saleManId,
                    FoodNetBalance = 0,
                    DeliveryFeeBalance = 0,
                    UpdatedAt = Key.DateTimeIQ
                };
                await _context.SaleManBalance.AddAsync(balance);
            }
            else
            {
                balance.FoodNetBalance = 0;
                balance.DeliveryFeeBalance = 0;
                balance.UpdatedAt = Key.DateTimeIQ;
                _context.Entry(balance).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return Result.Return(true, "تم تصفير حساب المندوب بنجاح — بقيت بياناته وطلباته محفوظة", new
            {
                settlement.SettlementId,
                saleManId,
                name = driver.Name,
                foodNetAmount = settlement.FoodNetAmount,
                deliveryFeeAmount = settlement.DeliveryFeeAmount,
                settledAt = settlement.SettledAt,
                ordersCount = openEntries.Count,
                foodNetBalance = 0m,
                deliveryFeeBalance = 0m
            });
        }

        public async Task<ResObj> GetSettlementsAsync(int? saleManId, DateTime? from, DateTime? to, string? name)
        {
            var q = from s in _context.SaleManSettlement.AsNoTracking()
                    join d in _context.SaleMan.AsNoTracking() on s.SaleManId equals d.SaleManId
                    select new { s, d };

            if (saleManId is > 0)
                q = q.Where(x => x.s.SaleManId == saleManId.Value);

            if (from.HasValue)
            {
                var f = from.Value.Date;
                q = q.Where(x => x.s.SettledAt >= f);
            }
            if (to.HasValue)
            {
                var t = to.Value.Date.AddDays(1).AddTicks(-1);
                q = q.Where(x => x.s.SettledAt <= t);
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                var n = name.Trim();
                q = q.Where(x =>
                    (x.d.Name != null && x.d.Name.Contains(n)) ||
                    (x.d.Phone != null && x.d.Phone.Contains(n)));
            }

            var rows = await q
                .OrderByDescending(x => x.s.SettledAt)
                .Take(500)
                .ToListAsync();

            var adminIds = rows
                .Where(x => x.s.SettledByAdminId != null)
                .Select(x => x.s.SettledByAdminId!.Value)
                .Distinct()
                .ToList();
            var admins = adminIds.Count == 0
                ? new Dictionary<int, string>()
                : await _context.Admin.AsNoTracking()
                    .Where(a => adminIds.Contains(a.AdminId))
                    .ToDictionaryAsync(a => a.AdminId, a => a.AdminName ?? ("#" + a.AdminId));

            var driverIds = rows.Select(x => x.s.SaleManId).Distinct().ToList();
            var balances = await GetBalancesMapAsync(driverIds);

            var settlementIds = rows.Select(x => x.s.SettlementId).ToList();
            var orderCounts = settlementIds.Count == 0
                ? new Dictionary<int, int>()
                : await _context.SaleManBalanceEntry.AsNoTracking()
                    .Where(e => e.SettlementId != null && settlementIds.Contains(e.SettlementId.Value))
                    .GroupBy(e => e.SettlementId!.Value)
                    .Select(g => new { SettlementId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.SettlementId, x => x.Count);

            var list = rows.Select(x =>
            {
                balances.TryGetValue(x.s.SaleManId, out var bal);
                orderCounts.TryGetValue(x.s.SettlementId, out var oc);
                string? adminName = null;
                if (x.s.SettledByAdminId is int aid)
                    admins.TryGetValue(aid, out adminName);

                return new SaleManSettlement
                {
                    SettlementId = x.s.SettlementId,
                    SaleManId = x.s.SaleManId,
                    FoodNetAmount = x.s.FoodNetAmount,
                    DeliveryFeeAmount = x.s.DeliveryFeeAmount,
                    SettledAt = x.s.SettledAt,
                    SettledByAdminId = x.s.SettledByAdminId,
                    Note = x.s.Note,
                    SaleManName = x.d.Name,
                    SaleManPhone = x.d.Phone,
                    AdminName = adminName,
                    CurrentFoodNetBalance = bal?.FoodNetBalance ?? 0m,
                    CurrentDeliveryFeeBalance = bal?.DeliveryFeeBalance ?? 0m,
                    OrdersCount = oc
                };
            }).ToList();

            return Result.Return(true, list);
        }

        public async Task<ResObj> GetSettlementOrdersAsync(int settlementId)
        {
            var settlement = await _context.SaleManSettlement.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettlementId == settlementId);
            if (settlement == null)
                return Result.Return(false, "عملية التصفير غير موجودة");

            var driver = await _context.SaleMan.AsNoTracking()
                .FirstOrDefaultAsync(d => d.SaleManId == settlement.SaleManId);

            var entries = await (
                from e in _context.SaleManBalanceEntry.AsNoTracking()
                join o in _context.Orders.AsNoTracking() on e.OrderId equals o.OrderId
                where e.SettlementId == settlementId
                orderby e.CreatedAt descending
                select new SaleManBalanceEntry
                {
                    EntryId = e.EntryId,
                    SaleManId = e.SaleManId,
                    OrderId = e.OrderId,
                    FoodNetAmount = e.FoodNetAmount,
                    DeliveryFeeAmount = e.DeliveryFeeAmount,
                    CreatedAt = e.CreatedAt,
                    SettlementId = e.SettlementId,
                    OrderNo = o.OrderNo,
                    OrderDate = o.OrderDate,
                    RestaurantName = o.RestaurantName,
                    UserName = o.UserName,
                    Address = o.Address
                }).ToListAsync();

            // RestaurantName/UserName may be unmapped on Orders — join if empty
            if (entries.Any(e => string.IsNullOrEmpty(e.RestaurantName)))
            {
                var orderIds = entries.Select(e => e.OrderId).ToList();
                var meta = await (
                    from o in _context.Orders.AsNoTracking()
                    join r in _context.Restaurant.AsNoTracking() on o.RestaurantId equals r.RestaurantId into rj
                    from r in rj.DefaultIfEmpty()
                    join u in _context.Users.AsNoTracking() on o.UserId equals u.UserId into uj
                    from u in uj.DefaultIfEmpty()
                    where orderIds.Contains(o.OrderId)
                    select new
                    {
                        o.OrderId,
                        o.OrderNo,
                        o.OrderDate,
                        o.Address,
                        RestaurantName = r != null ? r.Name : null,
                        UserName = u != null ? u.Name : null
                    }).ToListAsync();
                var map = meta.ToDictionary(m => m.OrderId);
                foreach (var e in entries)
                {
                    if (!map.TryGetValue(e.OrderId, out var m)) continue;
                    e.OrderNo ??= m.OrderNo;
                    e.OrderDate ??= m.OrderDate;
                    e.Address ??= m.Address;
                    e.RestaurantName ??= m.RestaurantName;
                    e.UserName ??= m.UserName;
                }
            }

            return Result.Return(true, new
            {
                settlement = new
                {
                    settlement.SettlementId,
                    settlement.SaleManId,
                    saleManName = driver?.Name,
                    saleManPhone = driver?.Phone,
                    settlement.FoodNetAmount,
                    settlement.DeliveryFeeAmount,
                    settlement.SettledAt,
                    settlement.Note,
                    ordersCount = entries.Count
                },
                orders = entries
            });
        }

        public async Task<ResObj> GetOpenBalanceOrdersAsync(int saleManId)
        {
            var driver = await _context.SaleMan.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SaleManId == saleManId);
            if (driver == null || driver.IsDelete == true)
                return Result.Return(false, "المندوب غير موجود");

            var balance = await _context.SaleManBalance.AsNoTracking()
                .FirstOrDefaultAsync(b => b.SaleManId == saleManId);

            var entries = await (
                from e in _context.SaleManBalanceEntry.AsNoTracking()
                join o in _context.Orders.AsNoTracking() on e.OrderId equals o.OrderId
                where e.SaleManId == saleManId && e.SettlementId == null
                orderby e.CreatedAt descending
                select new SaleManBalanceEntry
                {
                    EntryId = e.EntryId,
                    SaleManId = e.SaleManId,
                    OrderId = e.OrderId,
                    FoodNetAmount = e.FoodNetAmount,
                    DeliveryFeeAmount = e.DeliveryFeeAmount,
                    CreatedAt = e.CreatedAt,
                    OrderNo = o.OrderNo,
                    OrderDate = o.OrderDate,
                    Address = o.Address
                }).ToListAsync();

            var orderIds = entries.Select(e => e.OrderId).ToList();
            if (orderIds.Count > 0)
            {
                var meta = await (
                    from o in _context.Orders.AsNoTracking()
                    join r in _context.Restaurant.AsNoTracking() on o.RestaurantId equals r.RestaurantId into rj
                    from r in rj.DefaultIfEmpty()
                    join u in _context.Users.AsNoTracking() on o.UserId equals u.UserId into uj
                    from u in uj.DefaultIfEmpty()
                    where orderIds.Contains(o.OrderId)
                    select new
                    {
                        o.OrderId,
                        RestaurantName = r != null ? r.Name : null,
                        UserName = u != null ? u.Name : null
                    }).ToListAsync();
                var map = meta.ToDictionary(m => m.OrderId);
                foreach (var e in entries)
                {
                    if (!map.TryGetValue(e.OrderId, out var m)) continue;
                    e.RestaurantName = m.RestaurantName;
                    e.UserName = m.UserName;
                }
            }

            return Result.Return(true, new
            {
                saleManId,
                name = driver.Name,
                phone = driver.Phone,
                foodNetBalance = balance?.FoodNetBalance ?? 0m,
                deliveryFeeBalance = balance?.DeliveryFeeBalance ?? 0m,
                orders = entries
            });
        }
    }
}