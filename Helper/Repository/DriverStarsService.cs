using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;
using RomanaWeb.Model;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Helper.Repository
{
    public class DriverStarsService : IDriverStarsService, IRegisterScopped
    {
        private readonly DB_Context _context;

        public DriverStarsService(DB_Context context)
        {
            _context = context;
        }

        public async Task<ResObj> GetAll(string? saleManName)
        {
            var query = from ds in _context.DriverStars
                        join sm in _context.SaleMan on ds.SaleManId equals sm.SaleManId into smJoin
                        from sm in smJoin.DefaultIfEmpty()
                        select new DriverStars
                        {
                            DriverStarsId = ds.DriverStarsId,
                            StarsCount = ds.StarsCount,
                            SaleManId = ds.SaleManId,
                            Comments = ds.Comments,
                            OrderId = ds.OrderId,
                            SaleManName = sm != null ? sm.Name : "",
                            UserName = ds.UserName
                        };

            if (!string.IsNullOrEmpty(saleManName))
            {
                query = query.Where(x => x.SaleManName.Contains(saleManName));
            }

            var items = await query.AsNoTracking().ToListAsync();
            return Result.Return(true, items);
        }

        public async Task<ResObj> GetBySaleManId(int saleManId)
        {
            var items = await (from ds in _context.DriverStars
                              where ds.SaleManId == saleManId
                              select ds).AsNoTracking().ToListAsync();
            return Result.Return(true, items);
        }

        public async Task<ResObj> Post(DriverStars driverStars)
        {
            if (driverStars.DriverStarsId == 0)
            {
                await _context.DriverStars.AddAsync(driverStars);
            }
            else
            {
                var item = await _context.DriverStars.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.DriverStarsId == driverStars.DriverStarsId);
                if (item != null)
                {
                    item.StarsCount = driverStars.StarsCount;
                    item.SaleManId = driverStars.SaleManId;
                    item.Comments = driverStars.Comments;
                    _context.Entry(item).State = EntityState.Modified;
                }
            }
            await _context.SaveChangesAsync();
            return Result.Return(true, driverStars);
        }

        public async Task<ResObj> Delete(int id)
        {
            var item = await _context.DriverStars.AsNoTracking()
                .FirstOrDefaultAsync(i => i.DriverStarsId == id);
            if (item != null)
            {
                _context.DriverStars.Remove(item);
                await _context.SaveChangesAsync();
                return Result.Return(true, "تم الحذف بنجاح");
            }
            return Result.Return(false, "لم يتم العثور على التقييم");
        }
    }
}
