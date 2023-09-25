using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class SubCategoriesService : MasterService, ISubCategoriesService, IRegisterScopped
    {                                                                 
        public SubCategoriesService(DB_Context dB_Context, IMapper mapper) : base(mapper, dB_Context)
        {                                        
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.SubCategories.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.SubCategoriesId == Id);
            if (item != null)
            {
                _Context.SubCategories.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }
            return Result.Return(false);
        }
        public async Task<ResObj> GetAll(string? Name)
        {

            var item = await _Context.SubCategories.AsSplitQuery().AsNoTracking().Where(i=>i.SubCategoriesName.Contains(Name)|| Name==null).ToListAsync();
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> GetById(int Id)
        {
            var item = await _Context.SubCategories.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.SubCategoriesId == Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> Post(SubCategories SubCategories)
        {
            if (SubCategories.SubCategoriesId == 0)
            {
                var check = await _Context.SubCategories.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.SubCategoriesName == SubCategories.SubCategoriesName);
                if (check != null)
                {
                    return Result.Return(false, "تم الحفظ سابقا");
                }

                await _Context.SubCategories.AddAsync(SubCategories);
            }
            else
            {
                var item = await _Context.SubCategories.FirstOrDefaultAsync(i => i.SubCategoriesId == SubCategories.SubCategoriesId);
                if (item != null)
                {
                    item.SubCategoriesName = SubCategories.SubCategoriesName;
                    _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", SubCategories);
        }
    }
}