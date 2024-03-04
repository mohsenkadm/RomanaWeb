using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class CityService : MasterService, ICityService, IRegisterScopped
    {

        private readonly IDapperRepository<City> _cityService;
        public CityService(DB_Context dB_Context, IMapper mapper,IDapperRepository<City> dapper) : base(mapper, dB_Context)
        {                                        
            _cityService = dapper;
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.City.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.CityId == Id);
            if (item != null)
            {
                _Context.City.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }
            return Result.Return(false);
        }
        public async Task<ResObj> GetAll(string? Name)
        {

            //var item = await _Context.City.AsSplitQuery().AsNoTracking().Where(i=>i.CityName.Contains(Name)|| Name==null).ToListAsync();
            var item = await _cityService.GetEntityListAsync("dbo.GetCityAll",new { Name });
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }              
        public async Task<ResObj> GetByCountriesId(int CountriesId)
        {

            var item = await _Context.City.AsSplitQuery().AsNoTracking().Where(i=>i.CountriesId== CountriesId).ToListAsync();
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> GetById(int Id)
        {
            var item = await _Context.City.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.CityId == Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> Post(City City)
        {
            if (City.CityId == 0)
            {
                var check = await _Context.City.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.CityName == City.CityName);
                if (check != null)
                {
                    return Result.Return(false, "تم الحفظ سابقا");
                }

                await _Context.City.AddAsync(City);
            }
            else
            {
                var item = await _Context.City.FirstOrDefaultAsync(i => i.CityId == City.CityId);
                if (item != null)
                {
                    item.CityName = City.CityName;
                    _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", City);
        }
    }
}