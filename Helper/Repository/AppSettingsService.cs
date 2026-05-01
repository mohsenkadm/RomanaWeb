using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class AppSettingsService : IAppSettingsService, IRegisterScopped
    {
        private readonly DB_Context _context;

        public AppSettingsService(DB_Context context)
        {
            _context = context;
        }

        public async Task<ResObj> Get()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new AppSettings { PricePerKm = 500, DefaultOrderCost = 3000 };
                await _context.AppSettings.AddAsync(settings);
                await _context.SaveChangesAsync();
            }
            return Result.Return(true, settings);
        }

        public async Task<ResObj> Update(AppSettings settings)
        {
            var existing = await _context.AppSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                await _context.AppSettings.AddAsync(settings);
            }
            else
            {
                existing.PricePerKm = settings.PricePerKm;
                existing.DefaultOrderCost = settings.DefaultOrderCost;
                existing.MinChargeKmThreshold = settings.MinChargeKmThreshold;
                existing.MinChargeAmount = settings.MinChargeAmount;
                existing.RoundingMode = string.IsNullOrWhiteSpace(settings.RoundingMode) ? "Ceil" : settings.RoundingMode;
                existing.ZoneMaxKm = settings.ZoneMaxKm;
                existing.ZoneMinKm = settings.ZoneMinKm;
                _context.Entry(existing).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح");
        }

        public async Task<ResObj> UpdatePricePerKm(decimal pricePerKm)
        {
            var existing = await _context.AppSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                existing = new AppSettings { PricePerKm = pricePerKm, DefaultOrderCost = 3000 };
                await _context.AppSettings.AddAsync(existing);
            }
            else
            {
                existing.PricePerKm = pricePerKm;
                _context.Entry(existing).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم تحديث سعر الكيلومتر بنجاح");
        }

        public async Task<ResObj> UpdateDefaultOrderCost(decimal defaultOrderCost)
        {
            var existing = await _context.AppSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                existing = new AppSettings { PricePerKm = 500, DefaultOrderCost = defaultOrderCost };
                await _context.AppSettings.AddAsync(existing);
            }
            else
            {
                existing.DefaultOrderCost = defaultOrderCost;
                _context.Entry(existing).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم تحديث كلفة التوصيل الافتراضية بنجاح");
        }

        public async Task<decimal> GetPricePerKm()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();
            return settings?.PricePerKm ?? 500;
        }

        public async Task<decimal> GetDefaultOrderCost()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();
            return settings?.DefaultOrderCost ?? 3000;
        }

        public async Task<ResObj> CalculateDeliveryCostByLocation(double storeLat, double storeLng, double customerLat, double customerLng)
        {
            double distanceKm = CalculateHaversineDistance(storeLat, storeLng, customerLat, customerLng);
            decimal pricePerKm = await GetPricePerKm();
            decimal cost = (decimal)distanceKm * pricePerKm;

            var result = new
            {
                DistanceKm = Math.Round(distanceKm, 2),
                PricePerKm = pricePerKm,
                TotalCost = Math.Round(cost, 0)
            };

            return Result.Return(true, result);
        }

        public async Task<ResObj> CalculateDeliveryCostByKm(double distanceKm)
        {
            decimal pricePerKm = await GetPricePerKm();
            decimal cost = (decimal)distanceKm * pricePerKm;

            var result = new
            {
                DistanceKm = Math.Round(distanceKm, 2),
                PricePerKm = pricePerKm,
                TotalCost = Math.Round(cost, 0)
            };

            return Result.Return(true, result);
        }

        private static double CalculateHaversineDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371; // Earth radius in km
            double dLat = ToRad(lat2 - lat1);
            double dLng = ToRad(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double deg) => deg * (Math.PI / 180);
    }
}
