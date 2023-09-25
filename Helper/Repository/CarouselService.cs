using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;          
using RomanaWeb.Model.General;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class CarouselService : ICarouselService, IRegisterScopped
    {
        // cotext only apply scopped 
        private readonly DB_Context _context;                             

        public CarouselService(
            DB_Context context )
        {
            _context = context;                   
        }

        public async Task<ResObj> GetAllApp()
        {
            List<Carousel> data = await _context.Carousel.AsSplitQuery().AsNoTracking().Where(i=>i.IsShow==true).ToListAsync() ;
            return Result.Return(true, data);
        }                            
        public async Task<ResObj> GetAll()
        {
            List<Carousel> data = await _context.Carousel.AsSplitQuery().AsNoTracking().ToListAsync() ;
            return Result.Return(true, data);
        }

        public async Task<ResObj> Post(Carousel Carousel)
        {
            if (Carousel.Image.IsEmpty())
                return Result.Return(false, "رجاءا اكتب التفاصيل بالغة العربية");
             
            await _context.Carousel.AddAsync(Carousel);
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم الحفظ بنجاح",Carousel);
        }

        public async Task<ResObj> Update(Carousel Carousel)
        {
            if (Carousel.Image.IsEmpty())
                return Result.Return(false, "رجاءا اكتب التفاصيل بالغة العربية");
            Carousel Carousel1 = await GetCarouselById(Carousel.CarouseId);
            if (Carousel1 is null)
                return Result.Return(false, "حدث خطا اثناء عملية جلب البيانات");
             
            Carousel1.Image = Carousel.Image;
            Carousel1.IsShow = Carousel.IsShow;
            Carousel1.Url = Carousel.Url;
            _context.Entry(Carousel1).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم الحفظ بنجاح",Carousel1);
        }



        public async Task<ResObj> Delete(int Id)
        {
            Carousel Carousel1 = await GetCarouselById(Id);
            _context.Entry(Carousel1).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<Carousel> GetCarouselById(int Id)
        {
            return await _context.Carousel.AsSplitQuery().AsNoTracking().Where(i => i.CarouseId == Id).FirstOrDefaultAsync();
        }

        public async Task<ResObj> GetById(int Id)
        {
            Carousel Carousel = await GetCarouselById(Id);
            return Result.Return(true, Carousel);
        }
    }
}
