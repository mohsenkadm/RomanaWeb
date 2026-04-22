using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IAppSettingsService
    {
        Task<ResObj> Get();
        Task<ResObj> UpdatePricePerKm(decimal pricePerKm);
        Task<ResObj> UpdateDefaultOrderCost(decimal defaultOrderCost);
        Task<ResObj> Update(AppSettings settings);
        Task<decimal> GetPricePerKm();
        Task<decimal> GetDefaultOrderCost();
        Task<ResObj> CalculateDeliveryCostByLocation(double storeLat, double storeLng, double customerLat, double customerLng);
        Task<ResObj> CalculateDeliveryCostByKm(double distanceKm);
    }
}
