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
        public async Task<ResObj> Post(RestaurantSaleMan RestaurantSaleMan)
        {
            if (RestaurantSaleMan.RestaurantSaleManId == 0)
            {
                var check = await _Context.RestaurantSaleMan.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantId == RestaurantSaleMan.RestaurantId );
                if (check != null)
                {
                    return Result.Return(false, "تم الحفظ سابقا");
                }                                    
                await _Context.RestaurantSaleMan.AddAsync(RestaurantSaleMan);
            }
            else
            {
                var item = await _Context.RestaurantSaleMan.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantSaleManId == RestaurantSaleMan.RestaurantSaleManId);
                if (item != null)
                {
                     item.RestaurantId = RestaurantSaleMan.RestaurantId;
                     _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", RestaurantSaleMan);
        }
    }
}
