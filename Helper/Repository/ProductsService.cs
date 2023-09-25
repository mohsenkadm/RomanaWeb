using RomanaWeb.Classes;
using RomanaWeb.Models.Entity; 
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Helper.Interface;   
using RomanaWeb.Model;

namespace RomanaWeb.Helper.Repository
{
    public class ProductsService : IProductsService, IRegisterScopped
    {
        // cotext only apply scopped 
        private readonly DB_Context _context;
        private readonly IDapperRepository<Products> _prodService;     

        public ProductsService(
            DB_Context context, IDapperRepository<Products> prodService)
        {
            _context = context;
            _prodService = prodService;        
        }

        public async Task<ResObj> GetByRestaurantId(int RestaurantId, int UserId, int? SubCategoriesId)
        {
            List<Products> items = await _context.Products.AsSplitQuery().AsNoTracking().Where(i=>i.RestaurantId==RestaurantId && i.SubCategoriesId==SubCategoriesId).ToListAsync();
            if (items != null)
            {    
                return Result.Return(true, items);
            }
            else
                return Result.Return(false);   
        }

        public async Task<ResObj> Post(Products Products)
        {
            await _context.Products.AddAsync(Products);
            await _context.SaveChangesAsync();
           
            return Result.Return(true, "تم الحفظ بنجاح", Products.ProductsId);
        }

        public async Task<ResObj> Update(Products Products)
        {
            Products Products1 = await GetProductsById(Products.ProductsId);
            if (Products1 is null)
                return Result.Return(false, "حدث خطا اثناء عملية جلب البيانات");

            Products1.ProductsName = Products.ProductsName;
            Products1.ProductsPrice = Products.ProductsPrice;
            Products1.ProductsDetails = Products.ProductsDetails;
            Products1.SubCategoriesId = Products.SubCategoriesId;
            if(Products.ProductsImage.Length>0)
            Products1.ProductsImage = Products.ProductsImage;     

            _context.Entry(Products1).State = EntityState.Modified;
            await _context.SaveChangesAsync();
          
            return Result.Return(true, "تم الحفظ بنجاح", Products1);
        }
                   
        public async Task<ResObj> Delete(int Id)
        {
            Products Products1 = await GetProductsById(Id);
            _context.Entry(Products1).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<ResObj> GetAll(string? Name, string? RestaurantName, string? SubCategoriesName,int index)
        {
            List<Products> items = await _prodService.GetEntityListAsync("dbo.GetProductsAll", new { Name, RestaurantName, SubCategoriesName, index });
            if (items != null)
            {
                return Result.Return(true, items);
            }
            else
                return Result.Return(false);

        }
                                               
        public async Task<Products> GetProductsById(int Id)
        {
            return await _context.Products.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i=>i.ProductsId==Id);
        }

        public async Task<ResObj> GetById(int Id)
        {
            Products items = await GetProductsById(Id);
            if (items != null)
            {
                 return Result.Return(true, items);
            }
            else
                return Result.Return(false);     
        }

      
                                    
    }
}

