using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Controllers
{
    /// <summary>
    /// Dual balances + تصفير for delivery agents (SaleMan).
    /// </summary>
    public class SaleManBalanceController : MasterController
    {
        private readonly ISaleManBalanceService _balance;
        private readonly ILoggerRepository _logger;

        public SaleManBalanceController(ISaleManBalanceService balance, ILoggerRepository logger)
        {
            _balance = balance;
            _logger = logger;
        }

        private bool IsDriver() =>
            UserManager != null && string.Equals(UserManager.Role, "sal", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Driver app: current dual balance boxes.
        /// GET /SaleMan/MyBalance
        /// </summary>
        [Authorize]
        [HttpGet("SaleMan/MyBalance")]
        public async Task<IActionResult> MyBalance()
        {
            try
            {
                if (!IsDriver())
                    return Response(false, "هذه العملية مخصصة لمندوب التوصيل فقط");

                int saleManId = UserManager?.Id ?? 0;
                if (saleManId <= 0)
                    return Response(false, "غير مصرح");

                ResObj res = await _balance.GetBalanceAsync(saleManId);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManBalanceController => MyBalance");
                return Response(false, "حدث خطأ");
            }
        }

        /// <summary>Admin: get one driver's balances.</summary>
        [Authorize]
        [HttpGet("SaleMan/GetBalance/{Id}")]
        public async Task<IActionResult> GetBalance(int Id)
        {
            try
            {
                if (!IsAdminRole())
                    return ForbidAdminOnly();

                ResObj res = await _balance.GetBalanceAsync(Id);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManBalanceController => GetBalance");
                return Response(false, "حدث خطأ");
            }
        }

        /// <summary>Admin: open (unsettled) orders currently in the driver's balance.</summary>
        [Authorize]
        [HttpGet("SaleMan/GetOpenBalanceOrders/{Id}")]
        public async Task<IActionResult> GetOpenBalanceOrders(int Id)
        {
            try
            {
                if (!IsAdminRole())
                    return ForbidAdminOnly();

                ResObj res = await _balance.GetOpenBalanceOrdersAsync(Id);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManBalanceController => GetOpenBalanceOrders");
                return Response(false, "حدث خطأ");
            }
        }

        /// <summary>
        /// Admin: تصفير الحساب — zeros both balances, keeps all other data, writes settlement txn.
        /// POST /SaleMan/SettleBalance/{Id}?note=...
        /// </summary>
        [Authorize]
        [HttpPost("SaleMan/SettleBalance/{Id}")]
        public async Task<IActionResult> SettleBalance(int Id, [FromQuery] string? note = null)
        {
            try
            {
                if (!IsAdminRole())
                    return ForbidAdminOnly();

                int? adminId = UserManager?.Id;
                ResObj res = await _balance.SettleAsync(Id, adminId, note);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManBalanceController => SettleBalance");
                return Response(false, "حدث خطأ");
            }
        }

        /// <summary>Admin: settlement report list.</summary>
        [Authorize]
        [HttpGet("SaleMan/GetSettlements")]
        public async Task<IActionResult> GetSettlements(
            int? saleManId = null,
            DateTime? datefrom = null,
            DateTime? dateto = null,
            string? name = null)
        {
            try
            {
                if (!IsAdminRole())
                    return ForbidAdminOnly();

                ResObj res = await _balance.GetSettlementsAsync(saleManId, datefrom, dateto, name);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManBalanceController => GetSettlements");
                return Response(false, "حدث خطأ");
            }
        }

        /// <summary>Admin: orders that made up one settlement (تفاصيل).</summary>
        [Authorize]
        [HttpGet("SaleMan/GetSettlementOrders/{SettlementId}")]
        public async Task<IActionResult> GetSettlementOrders(int SettlementId)
        {
            try
            {
                if (!IsAdminRole())
                    return ForbidAdminOnly();

                ResObj res = await _balance.GetSettlementOrdersAsync(SettlementId);
                return Response(res.success, res.msg, res.data);
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "SaleManBalanceController => GetSettlementOrders");
                return Response(false, "حدث خطأ");
            }
        }
    }

    public class SettleBalanceDto
    {
        public string? Note { get; set; }
    }
}
