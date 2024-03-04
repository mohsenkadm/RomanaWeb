using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class CountriesService : MasterService, ICountriesService, IRegisterScopped
    {

        private readonly IDapperRepository<Countries> _CountriesService;
        public CountriesService(DB_Context dB_Context, IMapper mapper) : base(mapper, dB_Context)
        {                                        
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.Countries.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.CountriesId == Id);
            if (item != null)
            {
                _Context.Countries.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }
            return Result.Return(false);
        }
        public async Task<ResObj> GetAll()
        {

            var item = await _Context.Countries.AsSplitQuery().AsNoTracking().ToListAsync();
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> GetById(int Id)
        {
            var item = await _Context.Countries.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.CountriesId == Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> Post(Countries Countries)
        {
            if (Countries.CountriesId == 0)
            {
                var check = await _Context.Countries.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.CountriesName == Countries.CountriesName);
                if (check != null)
                {
                    return Result.Return(false, "تم الحفظ سابقا");
                }

                await _Context.Countries.AddAsync(Countries);
            }
            else
            {
                var item = await _Context.Countries.FirstOrDefaultAsync(i => i.CountriesId == Countries.CountriesId);
                if (item != null)
                {
                    item.CountriesName = Countries.CountriesName;
                    _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", Countries);
        }
    }
}