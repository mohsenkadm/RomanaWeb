using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;

namespace RomanaWeb.Helper.Repository
{
    /// <summary>
    /// Section 8 - Pure-EF analytics queries powering the "Data Analysis" dashboard.
    /// All queries scope to non-cancelled, completed orders unless noted.
    /// </summary>
    public class AnalyticsService : IAnalyticsService, IRegisterScopped
    {
        private readonly DB_Context _context;

        public AnalyticsService(DB_Context context)
        {
            _context = context;
        }

        public async Task<ResObj> OrdersOverTime(DateTime from, DateTime to, string bucket)
        {
            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.OrderDate >= from && o.OrderDate <= to && !o.IsCancel)
                .Select(o => new { o.OrderDate, o.NetAmount })
                .ToListAsync();

            var grouped = orders
                .GroupBy(o => Bucket(o.OrderDate, bucket))
                .OrderBy(g => g.Key)
                .Select(g => new { period = g.Key, count = g.Count(), revenue = g.Sum(x => x.NetAmount) })
                .ToList();

            return Result.Return(true, grouped);
        }

        public async Task<ResObj> Revenue(DateTime from, DateTime to)
        {
            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.OrderDate >= from && o.OrderDate <= to && !o.IsCancel)
                .Select(o => new { o.Total, o.TotalDiscount, o.NetAmount })
                .ToListAsync();

            var summary = new
            {
                gross = orders.Sum(o => o.Total),
                discounts = orders.Sum(o => o.TotalDiscount),
                net = orders.Sum(o => o.NetAmount),
                orders = orders.Count
            };
            return Result.Return(true, summary);
        }

        // Section 3.3 / 8: discounts funded by store vs by platform.
        // Heuristic: discount on an order is funded by store if its PromoCode maps to a
        // STORE-scoped promo (PromoCodes.IsForAllStores == false). Otherwise by platform.
        public async Task<ResObj> DiscountFunding(DateTime from, DateTime to)
        {
            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.OrderDate >= from && o.OrderDate <= to && !o.IsCancel && o.TotalDiscount > 0)
                .Select(o => new { o.PromoCode, o.TotalDiscount })
                .ToListAsync();

            var promoMap = await _context.PromoCodes.AsNoTracking()
                .Select(p => new { p.PromoName, p.IsForAllStores })
                .ToListAsync();
            var dict = promoMap
                .GroupBy(p => p.PromoName)
                .ToDictionary(g => g.Key ?? "", g => g.First().IsForAllStores);

            double byStore = 0, byPlatform = 0;
            foreach (var o in orders)
            {
                bool isGlobal = !string.IsNullOrEmpty(o.PromoCode) && dict.TryGetValue(o.PromoCode, out var g) && g;
                if (isGlobal) byPlatform += o.TotalDiscount;
                else byStore += o.TotalDiscount;
            }

            return Result.Return(true, new { funded_by_store = byStore, funded_by_platform = byPlatform });
        }

        public async Task<ResObj> TopStores(DateTime from, DateTime to, int take)
        {
            var top = await _context.Orders.AsNoTracking()
                .Where(o => o.OrderDate >= from && o.OrderDate <= to && !o.IsCancel)
                .GroupBy(o => o.RestaurantId)
                .Select(g => new { restaurantId = g.Key, orders = g.Count(), revenue = g.Sum(o => o.NetAmount) })
                .OrderByDescending(x => x.revenue)
                .Take(take <= 0 ? 10 : take)
                .ToListAsync();
            return Result.Return(true, top);
        }

        public async Task<ResObj> TopDrivers(DateTime from, DateTime to, int take)
        {
            var top = await _context.Orders.AsNoTracking()
                .Where(o => o.OrderDate >= from && o.OrderDate <= to && !o.IsCancel
                            && o.SaleManId != null && o.SaleManId > 0)
                .GroupBy(o => o.SaleManId!.Value)
                .Select(g => new { saleManId = g.Key, orders = g.Count() })
                .OrderByDescending(x => x.orders)
                .Take(take <= 0 ? 10 : take)
                .ToListAsync();
            return Result.Return(true, top);
        }

        public async Task<ResObj> TopPromoCodes(DateTime from, DateTime to, int take)
        {
            var top = await _context.Orders.AsNoTracking()
                .Where(o => o.OrderDate >= from && o.OrderDate <= to && !o.IsCancel
                            && o.PromoCode != null && o.PromoCode != "")
                .GroupBy(o => o.PromoCode!)
                .Select(g => new { promo = g.Key, uses = g.Count(), discount = g.Sum(o => o.TotalDiscount) })
                .OrderByDescending(x => x.uses)
                .Take(take <= 0 ? 10 : take)
                .ToListAsync();
            return Result.Return(true, top);
        }

        public async Task<ResObj> DeliveryStats(DateTime from, DateTime to)
        {
            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.OrderDate >= from && o.OrderDate <= to && !o.IsCancel)
                .Select(o => new { o.CostDelivery })
                .ToListAsync();

            var fees = orders.Where(o => o.CostDelivery.HasValue).Select(o => (double)o.CostDelivery!.Value).ToList();
            var summary = new
            {
                avg_delivery_fee = fees.Count == 0 ? 0d : fees.Average(),
                // distance_km isn't currently persisted on Orders; surface as null and
                // populate once the pricing engine writes the chosen distance to the order.
                avg_delivery_distance_km = (double?)null,
                deliveries = orders.Count
            };
            return Result.Return(true, summary);
        }

        public async Task<ResObj> RatingsOverview(DateTime from, DateTime to)
        {
            var stars = await _context.Stars.AsNoTracking()
                .Select(s => new { s.StarsCount, s.RestaurantId })
                .ToListAsync();
            var driverStars = await _context.DriverStars.AsNoTracking()
                .Select(s => new { s.StarsCount, s.SaleManId })
                .ToListAsync();

            var perStore = stars
                .GroupBy(s => s.RestaurantId ?? 0)
                .Select(g => new { restaurantId = g.Key, avg = g.Average(x => x.StarsCount ?? 0), count = g.Count() })
                .ToList();
            var perDriver = driverStars
                .GroupBy(s => s.SaleManId ?? 0)
                .Select(g => new { saleManId = g.Key, avg = g.Average(x => x.StarsCount ?? 0), count = g.Count() })
                .ToList();
            var distribution = stars.Concat(driverStars.Select(d => new { d.StarsCount, RestaurantId = (int?)null }))
                .GroupBy(s => s.StarsCount ?? 0)
                .OrderBy(g => g.Key)
                .Select(g => new { stars = g.Key, count = g.Count() })
                .ToList();

            return Result.Return(true, new { perStore, perDriver, distribution });
        }

        private static string Bucket(DateTime date, string bucket)
        {
            switch ((bucket ?? "day").ToLowerInvariant())
            {
                case "month": return date.ToString("yyyy-MM", CultureInfo.InvariantCulture);
                case "week":
                    var c = CultureInfo.InvariantCulture.Calendar;
                    int week = c.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    return $"{date:yyyy}-W{week:00}";
                default: return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }
    }
}
