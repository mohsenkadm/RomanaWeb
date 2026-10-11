using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface ISaleManBalanceService
    {
        /// <summary>
        /// Accrue food-net + delivery-fee into the driver's open balance when an order is delivered.
        /// Idempotent per OrderId.
        /// </summary>
        Task AccrueOnDeliveredAsync(Orders order);

        Task<ResObj> GetBalanceAsync(int saleManId);

        Task<Dictionary<int, SaleManBalance>> GetBalancesMapAsync(IEnumerable<int> saleManIds);

        /// <summary>تصفير الحساب — zeros both balances, keeps driver/orders data, writes settlement + links entries.</summary>
        Task<ResObj> SettleAsync(int saleManId, int? adminId, string? note);

        Task<ResObj> GetSettlementsAsync(int? saleManId, DateTime? from, DateTime? to, string? name);

        Task<ResObj> GetSettlementOrdersAsync(int settlementId);

        Task<ResObj> GetOpenBalanceOrdersAsync(int saleManId);
    }
}
