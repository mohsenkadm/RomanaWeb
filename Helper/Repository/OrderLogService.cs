using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class OrderLogService : IOrderLogService, IRegisterScopped
    {
        private readonly DB_Context _context;
        private readonly IHttpContextAccessor _http;

        public OrderLogService(DB_Context context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public async Task LogAsync(int orderId, string action, string title, string? details = null, int? orderNo = null)
        {
            if (orderId <= 0) return;
            try
            {
                if (orderNo == null || orderNo == 0)
                {
                    orderNo = await _context.Orders.AsNoTracking()
                        .Where(o => o.OrderId == orderId)
                        .Select(o => (int?)o.OrderNo)
                        .FirstOrDefaultAsync();
                }

                var actor = OrderAuditLogger.ResolveActor(_http);
                _context.OrderLogs.Add(new OrderLog
                {
                    OrderId = orderId,
                    OrderNo = orderNo,
                    Action = action ?? "event",
                    Title = title ?? "",
                    Details = details,
                    ActorType = actor.Type,
                    ActorId = actor.Id,
                    ActorName = actor.Name,
                    CreatedAt = Key.DateTimeIQ
                });

                // Suppress auto-audit (OrderLog itself isn't tracked by Capture, but keep safe)
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Never break callers.
            }
        }
    }
}
