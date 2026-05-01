using RomanaWeb.Classes;

namespace RomanaWeb.Helper.Interface
{
    /// <summary>Section 8 - Analytics dashboard data source.</summary>
    public interface IAnalyticsService
    {
        Task<ResObj> OrdersOverTime(DateTime from, DateTime to, string bucket); // day/week/month
        Task<ResObj> Revenue(DateTime from, DateTime to);
        Task<ResObj> DiscountFunding(DateTime from, DateTime to);
        Task<ResObj> TopStores(DateTime from, DateTime to, int take);
        Task<ResObj> TopDrivers(DateTime from, DateTime to, int take);
        Task<ResObj> TopPromoCodes(DateTime from, DateTime to, int take);
        Task<ResObj> DeliveryStats(DateTime from, DateTime to);
        Task<ResObj> RatingsOverview(DateTime from, DateTime to);
    }
}
