using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class RestaurantSubCategoriesService : MasterService, IRestaurantSubCategoriesService, IRegisterScopped
    {
        private readonly IDapperRepository<RestaurantSubCategories> _Repository;
        public RestaurantSubCategoriesService(DB_Context dB_Context, IDapperRepository<RestaurantSubCategories> dapperRepository, IMapper mapper) : base(mapper, dB_Context)
        {
            _Repository = dapperRepository;
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.RestaurantSubCategories.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantSubCategoriesId == Id);
            if (item != null)
            {
                _Context.RestaurantSubCategories.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }
            return Result.Return(false);
        }           
        public async Task<ResObj> GetAll(string? Name)
        {

            var item = await _Repository.GetEntityListAsync("dbo.GetRestaurantSubCategoriesAll", new { Name });
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }                      
        public async Task<ResObj> GetById(int Id)
        {
            var item = await _Context.RestaurantSubCategories.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantSubCategoriesId == Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }         
        public async Task<ResObj> Post(RestaurantSubCategories RestaurantSubCategories)
        {
            if (RestaurantSubCategories.RestaurantSubCategoriesId == 0)
            {
                var check = await _Context.RestaurantSubCategories.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantId == RestaurantSubCategories.RestaurantId && i.SubCategoriesId == RestaurantSubCategories.SubCategoriesId);
                if (check != null)
                {
                    return Result.Return(false, "تم الحفظ سابقا");
                }                                    
                await _Context.RestaurantSubCategories.AddAsync(RestaurantSubCategories);
            }
            else
            {
                var item = await _Context.RestaurantSubCategories.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.RestaurantSubCategoriesId == RestaurantSubCategories.RestaurantSubCategoriesId);
                if (item != null)
                {
                    item.SubCategoriesId = RestaurantSubCategories.SubCategoriesId;
                    item.RestaurantId = RestaurantSubCategories.RestaurantId;
                     _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", RestaurantSubCategories);
        }
    }
}
