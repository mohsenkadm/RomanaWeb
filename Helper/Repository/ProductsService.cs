using RomanaWeb.Classes;
using RomanaWeb.Models.Entity; 
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Helper.Interface;   
using RomanaWeb.Model;
using RomanaWeb.Models.EntityMapper;

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

        public async Task<ResObj> GetByRestaurantId(int RestaurantId, int? SubCategoriesId,string? prodname)
        {
            if (string.IsNullOrWhiteSpace(prodname) || prodname == "-" || prodname == "_")
                prodname = null;

            List<Products> items = await _prodService.GetEntityListAsync("dbo.GetProductsByRestaurantId", new { RestaurantId, SubCategoriesId, prodname });
                        
            if (items != null)
            {    
                return Result.Return(true, items);
            }
            else
                return Result.Return(false);   
        }

        public async Task<ResObj> Post(Products Products)
        {
            if (Products.ProductsDetails == null)
            {
                Products.ProductsDetails = "";
            }  
            if (Products.IsFree == null)
            {
                Products.IsFree = true;
            }   
            await _context.Products.AddAsync(Products);
            await _context.SaveChangesAsync();
           
            return Result.Return(true, "تم الحفظ بنجاح", Products.ProductsId);
        }

        public async Task<ResObj> PostBulk(QuickProductsBulkRequest request, string placeholderImageUrl)
        {
            var saved = new List<object>();
            var failed = new List<object>();

            if (request?.Items == null || request.Items.Count == 0)
                return Result.Return(false, "لا توجد منتجات للحفظ");

            if (request.RestaurantId <= 0)
                return Result.Return(false, "رجاءاً اختر المطعم");

            for (var i = 0; i < request.Items.Count; i++)
            {
                var item = request.Items[i];
                var name = (item.ProductsName ?? "").Trim();
                if (string.IsNullOrEmpty(name))
                {
                    failed.Add(new { index = i, name, reason = "اسم المنتج مطلوب" });
                    continue;
                }
                if (item.ProductsPrice <= 0)
                {
                    failed.Add(new { index = i, name, reason = "السعر يجب أن يكون أكبر من صفر" });
                    continue;
                }

                var subCatId = item.SubCategoriesId ?? request.DefaultSubCategoriesId ?? 0;
                if (subCatId <= 0)
                {
                    failed.Add(new { index = i, name, reason = "الصنف مطلوب" });
                    continue;
                }

                var product = new Products
                {
                    ProductsName = name,
                    ProductsPrice = (double)item.ProductsPrice,
                    ProductsDetails = item.ProductsDetails ?? "",
                    RestaurantId = request.RestaurantId,
                    SubCategoriesId = subCatId,
                    PreparationTimeMinutes = request.DefaultPreparationTimeMinutes > 0
                        ? request.DefaultPreparationTimeMinutes
                        : 15,
                    IsAvailable = true,
                    IsFree = true,
                    ProductsImageFirst = !string.IsNullOrWhiteSpace(item.ImageUrl)
                        ? item.ImageUrl
                        : placeholderImageUrl
                };

                try
                {
                    var res = await Post(product);
                    if (res.success)
                    {
                        var productId = Convert.ToInt32(res.data);
                        var imagePath = !string.IsNullOrWhiteSpace(item.ImageUrl)
                            ? item.ImageUrl
                            : placeholderImageUrl;

                        await PostImages(new Images()
                        {
                            ProductsId = productId,
                            ImagePath = imagePath
                        });

                        saved.Add(new
                        {
                            index = i,
                            name,
                            productsId = productId
                        });
                    }
                    else
                    {
                        failed.Add(new { index = i, name, reason = res.msg ?? "فشل الحفظ" });
                    }
                }
                catch (Exception ex)
                {
                    failed.Add(new { index = i, name, reason = ex.Message });
                }
            }

            return Result.Return(true, new { saved, failed });
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
            Products1.IsFree = Products.IsFree;
            Products1.PreparationTimeMinutes = Products.PreparationTimeMinutes;
            Products1.IsAvailable = Products.IsAvailable;
            if (Products.ProductsImageFirst!=null)
            {
                Products1.ProductsImageFirst = Products.ProductsImageFirst;
            }

            _context.Entry(Products1).State = EntityState.Modified;
            await _context.SaveChangesAsync();
          
            return Result.Return(true, "تم الحفظ بنجاح", Products1.ProductsId);
        }
                   
        public async Task<ResObj> Delete(int Id)
        {
            Products Products1 = await GetProductsById(Id);

            _context.Entry(Products1).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
            await DeleteImageForProd(Id);

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<ResObj> GetAll(string? Name, string? RestaurantName, string? SubCategoriesName, int index)
        {
            Name = string.IsNullOrWhiteSpace(Name) ? null : Name.Trim();
            RestaurantName = string.IsNullOrWhiteSpace(RestaurantName) ? null : RestaurantName.Trim();
            SubCategoriesName = string.IsNullOrWhiteSpace(SubCategoriesName) ? null : SubCategoriesName.Trim();

            List<Products> items = await _prodService.GetEntityListAsync("dbo.GetProductsAll", new { Name, RestaurantName, SubCategoriesName, index });
            if (items != null)
            {
                return Result.Return(true, items);
            }
            else
                return Result.Return(false);
        }
        public async Task<ResObj> GetAllBySearch(string? Name, int index)
        {
            List<Products> items = await _prodService.GetEntityListAsync("dbo.GetProductsAll", new { Name, index });
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
            Products items = await _prodService.GetEntityAsync("dbo.GetProductsById", new { Id});
            items.Images = await _context.Images.Where(i => i.ProductsId == Id).
                AsNoTracking().AsSplitQuery().ToListAsync();
            items.Sizes = await _context.ProductSize.Where(s => s.ProductsId == Id).AsNoTracking().ToListAsync();
            items.Ingredients = await _context.ProductIngredient.Where(i => i.ProductsId == Id).AsNoTracking().ToListAsync();
            if (items != null)
            {
                return Result.Return(true, items);
            }
            else
                return Result.Return(false);
        }



        public async Task<ResObj> SetIsFree(int id, bool IsFree)
        {
            var res = await GetProductsById(id);
            res.IsFree = IsFree;                                              

            _context.Entry(res).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم", res);
        }

        public async Task<ResObj> SetIsAvailable(int id, bool isAvailable)
        {
            var res = await _context.Products.FirstOrDefaultAsync(p => p.ProductsId == id);
            if (res == null) return Result.Return(false, "المنتج غير موجود");
            res.IsAvailable = isAvailable;
            _context.Entry(res).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, isAvailable ? "المنتج متوفر" : "المنتج غير متوفر", res);
        }

        public async Task<ResObj> GetTopSellingByRestaurant(int restaurantId, int take = 20)
        {
            var top = await (
                from d in _context.OrderDetail
                join o in _context.Orders on d.OrderId equals o.OrderId
                where o.RestaurantId == restaurantId && !o.IsCancel
                group d by d.ProductsId into g
                orderby g.Sum(x => x.Count) descending
                select new { ProductsId = g.Key, TotalOrdered = g.Sum(x => x.Count) }
            ).Take(take).ToListAsync();

            var ids = top.Select(t => t.ProductsId).ToList();
            var products = await _context.Products.AsNoTracking()
                .Where(p => ids.Contains(p.ProductsId))
                .ToListAsync();

            var result = top.Select(t =>
            {
                var p = products.FirstOrDefault(x => x.ProductsId == t.ProductsId);
                return new
                {
                    t.ProductsId,
                    t.TotalOrdered,
                    ProductsName = p?.ProductsName,
                    ProductsPrice = p?.ProductsPrice,
                    PreparationTimeMinutes = p?.PreparationTimeMinutes ?? 15,
                    IsAvailable = p?.IsAvailable ?? true,
                    ProductsImageFirst = p?.ProductsImageFirst
                };
            }).ToList();

            return Result.Return(true, result);
        }
        public async Task<ResObj> DeleteImage(int id)
        {
            await _prodService.RunScriptAsync("delete from Images where ImageId=" + id);
            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<ResObj> DeleteImageForProd(int id)
        {
            await _prodService.RunScriptAsync("delete from Images where ProductsId=" + id);
            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<ResObj> PostImages(Images images)
        {
            await _context.AddAsync(images);
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم الحفظ بنجاح");
        }

        public async Task<ResObj> GetImagesByProductsId(int Id)
        {
            List<Images> img = await _context.Images.Where(i => i.ProductsId == Id).ToListAsync();
            return Result.Return(true, img);
        }

        // --- Product Sizes ---

        public async Task<ResObj> GetSizesByProductId(int productId)
        {
            var sizes = await _context.ProductSize.Where(s => s.ProductsId == productId).ToListAsync();
            return Result.Return(true, sizes);
        }

        public async Task<ResObj> PostSize(ProductSize size)
        {
            await _context.ProductSize.AddAsync(size);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", size.ProductSizeId);
        }

        public async Task<ResObj> DeleteSize(int sizeId)
        {
            var size = await _context.ProductSize.FirstOrDefaultAsync(s => s.ProductSizeId == sizeId);
            if (size == null) return Result.Return(false, "السايز غير موجود");
            _context.ProductSize.Remove(size);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحذف بنجاح");
        }

        // --- Product Ingredients ---

        public async Task<ResObj> GetIngredientsByProductId(int productId)
        {
            var ingredients = await _context.ProductIngredient.Where(i => i.ProductsId == productId).ToListAsync();
            return Result.Return(true, ingredients);
        }

        public async Task<ResObj> PostIngredient(ProductIngredient ingredient)
        {
            await _context.ProductIngredient.AddAsync(ingredient);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", ingredient.ProductIngredientId);
        }

        public async Task<ResObj> DeleteIngredient(int ingredientId)
        {
            var ingredient = await _context.ProductIngredient.FirstOrDefaultAsync(i => i.ProductIngredientId == ingredientId);
            if (ingredient == null) return Result.Return(false, "المكون غير موجود");
            _context.ProductIngredient.Remove(ingredient);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحذف بنجاح");
        }
    }
}

