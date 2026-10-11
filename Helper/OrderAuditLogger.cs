using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Repository;
using RomanaWeb.Model;
using RomanaWeb.Model.General;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper
{
    /// <summary>
    /// Captures Orders / OrderDetail changes from EF ChangeTracker and writes OrderLog rows.
    /// Failures never break the original SaveChanges.
    /// </summary>
    public static class OrderAuditLogger
    {
        private static readonly AsyncLocal<bool> Suppress = new();

        public static bool IsSuppressed => Suppress.Value;

        public static async Task WritePendingAsync(DB_Context context, IHttpContextAccessor? http, List<PendingLog> pending)
        {
            if (Suppress.Value) return;
            if (pending == null || pending.Count == 0) return;
            try
            {
                await WriteLogsAsync(context, http, pending);
            }
            catch
            {
                // Never fail the business operation because of audit.
            }
        }

        public static List<PendingLog> Capture(DB_Context context)
        {
            var list = new List<PendingLog>();

            foreach (var entry in context.ChangeTracker.Entries<Orders>().ToList())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                if (entry.State == EntityState.Added)
                {
                    list.Add(new PendingLog
                    {
                        Kind = LogKind.OrderCreated,
                        OrderRef = entry.Entity,
                        OrderId = entry.Entity.OrderId,
                        OrderNo = entry.Entity.OrderNo
                    });
                    continue;
                }

                if (entry.State == EntityState.Deleted)
                {
                    list.Add(new PendingLog
                    {
                        Kind = LogKind.OrderDeleted,
                        OrderId = entry.Entity.OrderId,
                        OrderNo = entry.Entity.OrderNo,
                        Title = "حذف الطلب نهائياً",
                        Action = "deleted",
                        Details = $"رقم الطلب: {entry.Entity.OrderNo}"
                    });
                    continue;
                }

                if (entry.State == EntityState.Modified)
                    list.AddRange(BuildOrderModifiedLogs(entry));
            }

            foreach (var entry in context.ChangeTracker.Entries<OrderDetail>().ToList())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var detail = entry.Entity;
                int orderId = detail.OrderId;
                if (orderId <= 0 && entry.State != EntityState.Added)
                    continue;

                if (entry.State == EntityState.Added)
                {
                    list.Add(new PendingLog
                    {
                        Kind = LogKind.DetailChanged,
                        OrderId = orderId,
                        OrderRef = null,
                        DetailRef = detail,
                        Action = "details_changed",
                        Title = "إضافة منتج للطلب",
                        Details = $"منتج رقم {detail.ProductsId} — الكمية {detail.Count} — السعر {detail.Price}"
                    });
                }
                else if (entry.State == EntityState.Deleted)
                {
                    list.Add(new PendingLog
                    {
                        Kind = LogKind.DetailChanged,
                        OrderId = orderId,
                        Action = "details_changed",
                        Title = "حذف منتج من الطلب",
                        Details = $"منتج رقم {detail.ProductsId} — الكمية {detail.Count}"
                    });
                }
                else if (entry.State == EntityState.Modified)
                {
                    var parts = new List<string>();
                    if (HasChanged(entry, nameof(OrderDetail.Count)))
                        parts.Add($"الكمية: {entry.OriginalValues[nameof(OrderDetail.Count)]} → {entry.CurrentValues[nameof(OrderDetail.Count)]}");
                    if (HasChanged(entry, nameof(OrderDetail.Price)))
                        parts.Add($"السعر: {entry.OriginalValues[nameof(OrderDetail.Price)]} → {entry.CurrentValues[nameof(OrderDetail.Price)]}");
                    if (parts.Count == 0) continue;
                    list.Add(new PendingLog
                    {
                        Kind = LogKind.DetailChanged,
                        OrderId = orderId,
                        Action = "details_changed",
                        Title = "تعديل منتج في الطلب",
                        Details = $"منتج رقم {detail.ProductsId} — " + string.Join(" | ", parts)
                    });
                }
            }

            return list;
        }

        private static IEnumerable<PendingLog> BuildOrderModifiedLogs(EntityEntry<Orders> entry)
        {
            var logs = new List<PendingLog>();
            var order = entry.Entity;
            int orderId = order.OrderId;
            int orderNo = order.OrderNo;

            // Status change
            var before = SnapshotForStatus(entry, original: true);
            var after = SnapshotForStatus(entry, original: false);
            int beforeCode = OrdersService.MapOrderStatus(before);
            int afterCode = OrdersService.MapOrderStatus(after);
            if (beforeCode != afterCode)
            {
                logs.Add(new PendingLog
                {
                    Kind = LogKind.StatusChanged,
                    OrderId = orderId,
                    OrderNo = orderNo,
                    Action = afterCode == 9 ? "cancelled" : "status_changed",
                    Title = afterCode == 9 ? "إلغاء الطلب" : "تغيير حالة الطلب",
                    Details = $"{StatusText(beforeCode)} → {StatusText(afterCode)}"
                });
            }

            // Driver assignment
            int? oldDriver = GetOrigInt(entry, nameof(Orders.SaleManId));
            int? newDriver = order.SaleManId;
            int oldD = oldDriver is > 0 ? oldDriver.Value : 0;
            int newD = newDriver is > 0 ? newDriver.Value : 0;
            if (oldD != newD)
            {
                logs.Add(new PendingLog
                {
                    Kind = LogKind.DriverChanged,
                    OrderId = orderId,
                    OrderNo = orderNo,
                    OldSaleManId = oldD > 0 ? oldD : null,
                    NewSaleManId = newD > 0 ? newD : null,
                    Action = oldD == 0 ? "driver_assigned" : (newD == 0 ? "driver_removed" : "driver_changed"),
                    Title = oldD == 0 ? "تعيين مندوب" : (newD == 0 ? "إزالة المندوب" : "تبديل المندوب")
                });
            }

            // Field changes
            var fieldParts = new List<string>();
            AppendField(fieldParts, entry, nameof(Orders.CostDelivery), "سعر التوصيل");
            AppendField(fieldParts, entry, nameof(Orders.Total), "المجموع");
            AppendField(fieldParts, entry, nameof(Orders.TotalDiscount), "الخصم");
            AppendField(fieldParts, entry, nameof(Orders.NetAmount), "الصافي");
            AppendField(fieldParts, entry, nameof(Orders.Notes), "ملاحظات");
            AppendField(fieldParts, entry, nameof(Orders.Reason), "سبب عدم التوصيل");
            AppendField(fieldParts, entry, nameof(Orders.Reason2), "سبب التأجيل");
            AppendField(fieldParts, entry, nameof(Orders.PromoCode), "كود الخصم");
            AppendField(fieldParts, entry, nameof(Orders.IsPromoUsageCommitted), "تثبيت استخدام الكود");
            AppendField(fieldParts, entry, nameof(Orders.Lat), "خط العرض");
            AppendField(fieldParts, entry, nameof(Orders.Long), "خط الطول");

            if (fieldParts.Count > 0)
            {
                logs.Add(new PendingLog
                {
                    Kind = LogKind.FieldsUpdated,
                    OrderId = orderId,
                    OrderNo = orderNo,
                    Action = "fields_updated",
                    Title = "تعديل حقول الطلب",
                    Details = string.Join(" | ", fieldParts)
                });
            }

            // If nothing meaningful changed but entity is Modified (flag-only already covered by status), skip.
            return logs;
        }

        private static void AppendField(List<string> parts, EntityEntry entry, string prop, string label)
        {
            if (!HasChanged(entry, prop)) return;
            var oldV = entry.OriginalValues[prop];
            var newV = entry.CurrentValues[prop];
            parts.Add($"{label}: {Fmt(oldV)} → {Fmt(newV)}");
        }

        private static string Fmt(object? v)
        {
            if (v == null || v is DBNull) return "—";
            if (v is bool b) return b ? "نعم" : "لا";
            if (v is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm");
            return v.ToString() ?? "—";
        }

        private static bool HasChanged(EntityEntry entry, string prop)
        {
            try
            {
                var p = entry.Property(prop);
                return p.IsModified && !Equals(p.OriginalValue, p.CurrentValue);
            }
            catch
            {
                return false;
            }
        }

        private static int? GetOrigInt(EntityEntry entry, string prop)
        {
            try
            {
                var v = entry.OriginalValues[prop];
                if (v == null || v is DBNull) return null;
                return Convert.ToInt32(v);
            }
            catch { return null; }
        }

        private static Orders SnapshotForStatus(EntityEntry<Orders> entry, bool original)
        {
            var vals = original ? entry.OriginalValues : entry.CurrentValues;
            return new Orders
            {
                IsCancel = GetBool(vals, nameof(Orders.IsCancel)),
                IsApporve = GetBool(vals, nameof(Orders.IsApporve)),
                IsDone = GetBool(vals, nameof(Orders.IsDone)),
                IsPreparing = GetBool(vals, nameof(Orders.IsPreparing)),
                IsSaleManApprove = GetBoolN(vals, nameof(Orders.IsSaleManApprove)),
                IsDriverEnRouteToPickup = GetBool(vals, nameof(Orders.IsDriverEnRouteToPickup)),
                IsPickedUpFromRestaurant = GetBool(vals, nameof(Orders.IsPickedUpFromRestaurant)),
                IsOutForDelivery = GetBool(vals, nameof(Orders.IsOutForDelivery)),
                IsDelivered = GetBoolN(vals, nameof(Orders.IsDelivered)),
                IsDeliveryConfirmed = GetBool(vals, nameof(Orders.IsDeliveryConfirmed))
            };
        }

        private static bool GetBool(PropertyValues vals, string name)
        {
            try
            {
                var v = vals[name];
                if (v == null || v is DBNull) return false;
                return Convert.ToBoolean(v);
            }
            catch { return false; }
        }

        private static bool? GetBoolN(PropertyValues vals, string name)
        {
            try
            {
                var v = vals[name];
                if (v == null || v is DBNull) return null;
                return Convert.ToBoolean(v);
            }
            catch { return null; }
        }

        public static string StatusText(int code) => code switch
        {
            0 => "انتظار الموافقة",
            1 => "موافق عليه",
            2 => "قيد التحضير",
            3 => "قبل السائق",
            4 => "السائق في الطريق للاستلام",
            5 => "تم الاستلام من المطعم",
            6 => "قيد التوصيل",
            7 => "تم التوصيل",
            8 => "تم تأكيد الاستلام",
            9 => "ملغي",
            _ => code.ToString()
        };

        private static async Task WriteLogsAsync(DB_Context context, IHttpContextAccessor? http, List<PendingLog> pending)
        {
            var actor = ResolveActor(http);

            // Resolve OrderId / OrderNo for newly created orders and details linked to them
            foreach (var p in pending)
            {
                if (p.OrderRef != null)
                {
                    p.OrderId = p.OrderRef.OrderId;
                    p.OrderNo = p.OrderRef.OrderNo;
                }
                if (p.DetailRef != null && p.OrderId <= 0)
                    p.OrderId = p.DetailRef.OrderId;
            }

            // Driver names
            var driverIds = pending
                .Where(p => p.Kind == LogKind.DriverChanged)
                .SelectMany(p => new[] { p.OldSaleManId ?? 0, p.NewSaleManId ?? 0 })
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            Dictionary<int, string> driverNames = new();
            if (driverIds.Count > 0)
            {
                try
                {
                    driverNames = await context.SaleMan.AsNoTracking()
                        .Where(s => driverIds.Contains(s.SaleManId))
                        .Select(s => new { s.SaleManId, s.Name })
                        .ToDictionaryAsync(s => s.SaleManId, s => s.Name ?? ("#" + s.SaleManId));
                }
                catch { }
            }

            // Fill OrderNo for detail-only logs
            var missingNos = pending.Where(p => p.OrderId > 0 && (p.OrderNo == null || p.OrderNo == 0)).Select(p => p.OrderId).Distinct().ToList();
            if (missingNos.Count > 0)
            {
                try
                {
                    var nos = await context.Orders.AsNoTracking()
                        .Where(o => missingNos.Contains(o.OrderId))
                        .Select(o => new { o.OrderId, o.OrderNo })
                        .ToDictionaryAsync(o => o.OrderId, o => o.OrderNo);
                    foreach (var p in pending)
                    {
                        if ((p.OrderNo == null || p.OrderNo == 0) && nos.TryGetValue(p.OrderId, out var n))
                            p.OrderNo = n;
                    }
                }
                catch { }
            }

            var now = Key.DateTimeIQ;
            var rows = new List<OrderLog>();

            foreach (var p in pending)
            {
                if (p.OrderId <= 0) continue;

                string? details = p.Details;
                string title = p.Title ?? "";
                string action = p.Action ?? "updated";

                if (p.Kind == LogKind.OrderCreated)
                {
                    action = "created";
                    title = "إنشاء الطلب";
                    details = $"رقم الطلب: {p.OrderNo}";
                }
                else if (p.Kind == LogKind.DriverChanged)
                {
                    string OldName() => p.OldSaleManId is > 0 && driverNames.TryGetValue(p.OldSaleManId.Value, out var n) ? n : (p.OldSaleManId?.ToString() ?? "—");
                    string NewName() => p.NewSaleManId is > 0 && driverNames.TryGetValue(p.NewSaleManId.Value, out var n) ? n : (p.NewSaleManId?.ToString() ?? "—");
                    if (p.OldSaleManId is null or 0)
                        details = $"المندوب: {NewName()}";
                    else if (p.NewSaleManId is null or 0)
                        details = $"المندوب السابق: {OldName()}";
                    else
                        details = $"{OldName()} → {NewName()}";
                }

                rows.Add(new OrderLog
                {
                    OrderId = p.OrderId,
                    OrderNo = p.OrderNo,
                    Action = action,
                    Title = title,
                    Details = details,
                    ActorType = actor.Type,
                    ActorId = actor.Id,
                    ActorName = actor.Name,
                    CreatedAt = now
                });
            }

            if (rows.Count == 0) return;

            Suppress.Value = true;
            try
            {
                await context.OrderLogs.AddRangeAsync(rows);
                await context.SaveChangesAsync();
            }
            finally
            {
                Suppress.Value = false;
            }
        }

        public static (string Type, int? Id, string? Name) ResolveActor(IHttpContextAccessor? http)
        {
            try
            {
                var user = http?.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                    return ("System", null, null);

                var claim = user.Claims.FirstOrDefault(c => c.Type == ClaimInfo.UserManager)?.Value;
                if (string.IsNullOrWhiteSpace(claim))
                    return ("System", null, null);

                var um = JsonConvert.DeserializeObject<UserManager>(claim);
                if (um == null) return ("System", null, null);

                string type = (um.Role ?? "").ToLowerInvariant() switch
                {
                    "admin" => "Admin",
                    "res" => "Restaurant",
                    "sal" => "Driver",
                    "user" => "Customer",
                    _ => "System"
                };
                return (type, um.Id, um.Name);
            }
            catch
            {
                return ("System", null, null);
            }
        }

        public enum LogKind
        {
            OrderCreated,
            OrderDeleted,
            StatusChanged,
            DriverChanged,
            FieldsUpdated,
            DetailChanged
        }

        public class PendingLog
        {
            public LogKind Kind { get; set; }
            public int OrderId { get; set; }
            public int? OrderNo { get; set; }
            public Orders? OrderRef { get; set; }
            public OrderDetail? DetailRef { get; set; }
            public string? Action { get; set; }
            public string? Title { get; set; }
            public string? Details { get; set; }
            public int? OldSaleManId { get; set; }
            public int? NewSaleManId { get; set; }
        }
    }
}
