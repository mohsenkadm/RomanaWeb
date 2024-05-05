using AutoMapper;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class RestaurantCityService : MasterService, IRestaurantCityService, IRegisterScopped
    {
        private readonly IDapperRepository<RestaurantCity> _Repository;
        public RestaurantCityService(DB_Context dB_Context, IDapperRepository<RestaurantCity> dapperRepository, IMapper mapper) : base(mapper, dB_Context)
        {
            _Repository = dapperRepository;
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.RestaurantCity.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantCityId == Id);
            if (item != null)
            {
                _Context.RestaurantCity.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }
            return Result.Return(false);
        }           
        public async Task<ResObj> GetAll(string? Name)
        {

            var item = await _Repository.GetEntityListAsync("dbo.GetRestaurantCityAll", new { Name });
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }                      
        public async Task<ResObj> GetById(int Id)
        {
            var item = await _Context.RestaurantCity.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantCityId == Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }                             
        public async Task<ResObj> GetByResId(int Id)
        {
            var item = await _Repository.GetEntityListAsync("dbo.GetRestaurantCityByResId", new { Id });
            if (item != null)
            return Result.Return(true, item);
            else
                return Result.Return(false);
        }         
        public async Task<ResObj> Post(RestaurantCity RestaurantCity)
        {
            if (RestaurantCity.RestaurantCityId == 0)
            {
                var check = await _Context.RestaurantCity.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantId == RestaurantCity.RestaurantId && i.CityId == RestaurantCity.CityId);
                if (check != null)
                {
                    return Result.Return(false, "تم الحفظ سابقا");
                }                                    
                await _Context.RestaurantCity.AddAsync(RestaurantCity);
            }
            else
            {
                var item = await _Context.RestaurantCity.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantCityId == RestaurantCity.RestaurantCityId);
                if (item != null)
                {
                    item.CityId = RestaurantCity.CityId;
                    item.RestaurantId = RestaurantCity.RestaurantId;
                    item.CostDelivery = RestaurantCity.CostDelivery;
                     _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", RestaurantCity);
        }                
        public async Task<ResObj> PostFromAppAll(int resId,decimal CostDelivery)
        {
            await _Repository.RunScriptAsync("INSERT INTO [dbo].[RestaurantCity]" +
                "           ([CityId],[RestaurantId],[CostDelivery]) " +
                "(select CityId,"+resId+","+ CostDelivery + " from City where CityId not in (select CityId from RestaurantCity where RestaurantId="+resId+"))");
            
                return Result.Return(true, "تم الحفظ");  
        }
    }
}
