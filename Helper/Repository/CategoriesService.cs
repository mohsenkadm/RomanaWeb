using RomanaWeb.Models.Entity;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;

namespace RomanaWeb.Helper.Repository
{
    public class CategoriesService : ICategoriesService, IRegisterScopped
    {
        // cotext only apply scopped 
        private readonly DB_Context _context;

        public CategoriesService(
            DB_Context context)
        {
            _context = context;
        }

        public async Task<ResObj> GetAll()
        {
            List<Categories> data = await _context.Categories.AsSplitQuery().AsNoTracking().OrderBy(x=>Guid.NewGuid()).ToListAsync();
            return Result.Return(true, data);
        }

        public async Task<ResObj> Post(Categories Categories)
        {
            await _context.Categories.AddAsync(Categories);
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم الحفظ بنجاح", Categories);
        }

        public async Task<ResObj> Update(Categories Categories)
        {
            Categories Categories1 = await GetCategoriesById(Categories.CategoriesId);
            if (Categories1 is null)
                return Result.Return(false, "حدث خطا اثناء عملية جلب البيانات");
            
            Categories1.CategoriesName = Categories.CategoriesName;
            Categories1.CategoriesImages = Categories.CategoriesImages;
                                            
            _context.Entry(Categories1).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم الحفظ بنجاح", Categories1);
        }



        public async Task<ResObj> Delete(int Id)
        {
            Categories Categories1 = await GetCategoriesById(Id);
            _context.Entry(Categories1).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<Categories> GetCategoriesById(int Id)
        {
            return await _context.Categories.AsSplitQuery().AsNoTracking().Where(i => i.CategoriesId == Id).FirstOrDefaultAsync();
        }

        public async Task<ResObj> GetById(int Id)
        {
            Categories Categories = await GetCategoriesById(Id);
            return Result.Return(true, Categories);
        }
    }
}
