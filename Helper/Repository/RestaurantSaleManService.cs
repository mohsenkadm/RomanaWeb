using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class RestaurantSaleManService : MasterService, IRestaurantSaleManService, IRegisterScopped
    {
        private readonly IDapperRepository<RestaurantSaleMan> _Repository;
        public RestaurantSaleManService(DB_Context dB_Context, IDapperRepository<RestaurantSaleMan> dapperRepository, IMapper mapper) : base(mapper, dB_Context)
        {
            _Repository = dapperRepository;
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.RestaurantSaleMan.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantSaleManId == Id);
            if (item != null)
            {
                _Context.RestaurantSaleMan.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }
            return Result.Return(false);
        }
        public async Task<ResObj> GetAll(string? Name)
        {

            var item = await _Repository.GetEntityListAsync("dbo.GetRestaurantSaleManAll", new { Name });
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> GetById(int Id)
        {
            var item = await _Context.RestaurantSaleMan.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantSaleManId == Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> GetByResId(int Id)
        {
            var item = await _Repository.GetEntityListAsync("dbo.GetRestaurantSaleManByResId", new { Id });
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }

        public async Task<ResObj> GetBySaleManId(int saleManId)
        {
            var ids = await _Context.RestaurantSaleMan.AsNoTracking()
                .Where(rs => rs.SaleManId == saleManId)
                .Select(rs => rs.RestaurantId)
                .ToListAsync();
            return Result.Return(true, ids);
        }

        public async Task<ResObj> SetSaleManRestaurants(int saleManId, int[] restaurantIds)
        {
            var saleMan = await _Context.SaleMan.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SaleManId == saleManId && s.IsDelete != true);
            if (saleMan == null)
                return Result.Return(false, "المندوب غير موجود");

            var existing = await _Context.RestaurantSaleMan
                .Where(rs => rs.SaleManId == saleManId)
                .ToListAsync();
            _Context.RestaurantSaleMan.RemoveRange(existing);

            var uniqueIds = (restaurantIds ?? Array.Empty<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            foreach (var rid in uniqueIds)
            {
                await _Context.RestaurantSaleMan.AddAsync(new RestaurantSaleMan
                {
                    SaleManId = saleManId,
                    RestaurantId = rid
                });
            }

            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم تحديث مطاعم المندوب", uniqueIds);
        }

        public async Task<ResObj> LinksSummary()
        {
            var restaurants = await _Context.Restaurant.AsNoTracking()
                .Where(r => r.IsDelete != true)
                .OrderBy(r => r.SortOrder == 0 ? int.MaxValue : r.SortOrder)
                .ThenBy(r => r.Name)
                .Select(r => new { r.RestaurantId, r.Name, r.IsActive, r.IsApproved })
                .ToListAsync();

            var links = await _Context.RestaurantSaleMan.AsNoTracking().ToListAsync();
            var bySaleMan = links
                .GroupBy(l => l.SaleManId)
                .ToDictionary(g => g.Key.ToString(), g => g.Select(x => x.RestaurantId).Distinct().ToList());
            var byRestaurant = links
                .GroupBy(l => l.RestaurantId)
                .ToDictionary(g => g.Key.ToString(), g => g.Select(x => x.SaleManId).Distinct().ToList());

            return Result.Return(true, new { restaurants, bySaleMan, byRestaurant });
        }

        public async Task<ResObj> Post(RestaurantSaleMan RestaurantSaleMan)
        {
            if (RestaurantSaleMan.RestaurantSaleManId == 0)
            {
                var check = await _Context.RestaurantSaleMan.AsSplitQuery().AsNoTracking()
                    .FirstOrDefaultAsync(i =>
                        i.RestaurantId == RestaurantSaleMan.RestaurantId
                        && i.SaleManId == RestaurantSaleMan.SaleManId);
                if (check != null)
                {
                    return Result.Return(false, "تم الحفظ سابقا");
                }
                await _Context.RestaurantSaleMan.AddAsync(RestaurantSaleMan);
            }
            else
            {
                var item = await _Context.RestaurantSaleMan.AsSplitQuery().AsNoTracking()
                    .FirstOrDefaultAsync(i => i.RestaurantSaleManId == RestaurantSaleMan.RestaurantSaleManId);
                if (item != null)
                {
                    item.RestaurantId = RestaurantSaleMan.RestaurantId;
                    item.SaleManId = RestaurantSaleMan.SaleManId;
                    _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", RestaurantSaleMan);
        }
    }
}
