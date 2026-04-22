using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IDriverStarsService
    {
        Task<ResObj> GetAll(string? saleManName);
        Task<ResObj> GetBySaleManId(int saleManId);
        Task<ResObj> Post(DriverStars driverStars);
        Task<ResObj> Delete(int id);
    }
}
