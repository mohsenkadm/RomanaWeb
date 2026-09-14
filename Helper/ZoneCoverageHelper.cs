using Microsoft.EntityFrameworkCore;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper
{
    /// <summary>PDF §2.2 / §2.3.1 — strict zone matching for restaurants and drivers.</summary>
    public static class ZoneCoverageHelper
    {
        public static bool ServesZone(IReadOnlyCollection<int>? assignedZoneIds, int? targetZoneId)
        {
            if (targetZoneId is null or <= 0) return false;
            if (assignedZoneIds == null || assignedZoneIds.Count == 0) return false;
            return assignedZoneIds.Contains(targetZoneId.Value);
        }

        public static async Task<int?> ResolveZoneIdAsync(
            IPricingService pricing, IDistanceService distance,
            string? latStr, string? lngStr)
        {
            if (!distance.TryParseCoord(latStr, lngStr, out double lat, out double lng))
                return null;
            var (_, zone) = await pricing.ResolveZoneAtPointAsync(lat, lng);
            return zone?.ZoneId;
        }

        public static async Task<int?> ResolveOrderDropoffZoneIdAsync(
            DB_Context context, IPricingService pricing, IDistanceService distance, Orders order)
        {
            var user = await context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == order.UserId);
            string? latStr = !string.IsNullOrWhiteSpace(order.Lat) ? order.Lat : user?.Lat;
            string? lngStr = !string.IsNullOrWhiteSpace(order.Long) ? order.Long : user?.Long;
            return await ResolveZoneIdAsync(pricing, distance, latStr, lngStr);
        }

        public static async Task<HashSet<int>> GetDriverZoneIdsAsync(DB_Context context, int saleManId)
        {
            var ids = await context.SaleManZone.AsNoTracking()
                .Where(sz => sz.SaleManId == saleManId)
                .Select(sz => sz.ZoneId)
                .ToListAsync();
            return ids.ToHashSet();
        }

        /// <summary>True if the driver is assigned to the order's restaurant (RestaurantSaleMan).</summary>
        public static bool ServesRestaurant(IReadOnlyCollection<int>? assignedRestaurantIds, int restaurantId)
        {
            if (restaurantId <= 0) return false;
            if (assignedRestaurantIds == null || assignedRestaurantIds.Count == 0) return false;
            return assignedRestaurantIds.Contains(restaurantId);
        }

        public static async Task<HashSet<int>> GetDriverRestaurantIdsAsync(DB_Context context, int saleManId)
        {
            var ids = await context.RestaurantSaleMan.AsNoTracking()
                .Where(rs => rs.SaleManId == saleManId)
                .Select(rs => rs.RestaurantId)
                .ToListAsync();
            return ids.ToHashSet();
        }

        public static async Task<HashSet<int>> GetRestaurantZoneIdsAsync(DB_Context context, int restaurantId)
        {
            var ids = await context.RestaurantZone.AsNoTracking()
                .Where(rz => rz.RestaurantId == restaurantId)
                .Select(rz => rz.ZoneId)
                .ToListAsync();
            return ids.ToHashSet();
        }

        /// <summary>Zones that have at least one available driver assigned.</summary>
        public static async Task<HashSet<int>> GetZonesWithAvailableDriversAsync(DB_Context context)
        {
            var availableDriverIds = await context.SaleMan.AsNoTracking()
                .Where(d => d.IsDelete != true && d.IsActive != false && d.IsAvailable)
                .Select(d => d.SaleManId)
                .ToListAsync();

            if (availableDriverIds.Count == 0)
                return new HashSet<int>();

            var zoneIds = await context.SaleManZone.AsNoTracking()
                .Where(sz => availableDriverIds.Contains(sz.SaleManId))
                .Select(sz => sz.ZoneId)
                .Distinct()
                .ToListAsync();

            return zoneIds.ToHashSet();
        }
    }
}
