using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IPromoCodeService
    {
        public Task<ResObj> GetAll(string? Name,int? ResId);
        public Task<ResObj> GetById(int Id);
        public Task<ResObj> GetByResId(int Id);
        public Task<ResObj> Delete(int Id);                                  
        public Task<ResObj> Post(PromoCode PromoCode);

    }
}
