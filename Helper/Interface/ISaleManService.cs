using RomanaWeb.Models.Entity;
using RomanaWeb.Classes;
using System.Runtime.InteropServices;

namespace RomanaWeb.Helper.Interface
{
    public interface ISaleManService
    {
        Task<ResObj> Login(string Phone, string password);       
        Task<ResObj> GetAll(string? Name);
        Task<ResObj> GetByRestaurantId(int RestaurantId);
        Task<ResObj> Post(SaleMan SaleMan);
        Task<ResObj> Update(SaleMan SaleMan);
        Task<ResObj> Delete(int Id);
        Task<SaleMan> GetSaleManById(int Id);
        Task<ResObj> GetById(int Id);       
        Task<ResObj> GetCountForSale(int Id, DateTime datefrom, DateTime dateto);
       
    }
}
