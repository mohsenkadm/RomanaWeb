using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class AppSplashService : IAppSplashService, IRegisterScopped
    {
        private readonly DB_Context _context;

        public AppSplashService(DB_Context context) => _context = context;

        public async Task<ResObj> GetForApp()
        {
            var row = await _context.AppSplash.AsNoTracking()
                .OrderByDescending(s => s.AppSplashId)
                .FirstOrDefaultAsync();

            if (row == null || !row.IsVisible)
                return Result.Return(true, (object?)null);

            return Result.Return(true, new { row.ImageUrl, row.Details, row.IsVisible });
        }

        public async Task<ResObj> GetAdmin()
        {
            var row = await _context.AppSplash.AsNoTracking()
                .OrderByDescending(s => s.AppSplashId)
                .FirstOrDefaultAsync();
            return Result.Return(true, row);
        }

        public async Task<ResObj> Save(AppSplash splash)
        {
            if (string.IsNullOrWhiteSpace(splash.ImageUrl))
                return Result.Return(false, "يجب اختيار صورة");

            var existing = await _context.AppSplash.OrderByDescending(s => s.AppSplashId).FirstOrDefaultAsync();
            if (existing == null)
            {
                splash.UpdatedAt = DateTime.UtcNow;
                await _context.AppSplash.AddAsync(splash);
            }
            else
            {
                existing.ImageUrl = splash.ImageUrl;
                existing.Details = splash.Details;
                existing.IsVisible = splash.IsVisible;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.Entry(existing).State = EntityState.Modified;
                splash = existing;
            }

            // Enforce single row: remove extras if any
            var all = await _context.AppSplash.OrderByDescending(s => s.AppSplashId).ToListAsync();
            if (all.Count > 1)
            {
                foreach (var extra in all.Skip(1))
                    _context.AppSplash.Remove(extra);
            }

            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", splash);
        }
    }
}
