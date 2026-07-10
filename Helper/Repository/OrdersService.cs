using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;
using RomanaWeb.Models.EntityMapper;
using RomanaWeb.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RomanaWeb.Helper;
using RomanaWeb.Helper.Interface;
using System.Globalization;
using static NuGet.Packaging.PackagingConstants;

namespace RomanaWeb.Helper.Repository
{
    public class OrdersService : IOrdersService, IRegisterScopped
    {
        private readonly DB_Context _context;
        private readonly IDapperRepository<Orders> _OrdersRepository;
        private readonly IDapperRepository<OrderDetail> _OrderDetailRepository;
        private readonly IDriverDispatchService _dispatch;
        private readonly IDistanceService _distance;
        private readonly IPricingService _pricing;
        private readonly IConfiguration _config;
        private readonly ILoggerRepository _logger;

        public OrdersService(
            DB_Context context,
            IDapperRepository<Orders> orderRepository,
            IDapperRepository<OrderDetail> OrderDetailRepository,
            IDriverDispatchService dispatch,
            IDistanceService distance,
            IPricingService pricing,
            IConfiguration config,
            ILoggerRepository logger)
        {
            _context = context;
            _OrdersRepository = orderRepository;
            _OrderDetailRepository = OrderDetailRepository;
            _dispatch = dispatch;
            _distance = distance;
            _pricing = pricing;
            _config = config;
            _logger = logger;
        }


        public async Task<ResObj> Delete(int Id)
        {
            Orders Orders1 = await GetOrdersById(Id);
            _context.Entry(Orders1).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
            List<OrderDetail> orderDetail = await GetOrdersDetailById(Id);
            foreach (var item in orderDetail)
            {
                _context.Entry(item).State = EntityState.Deleted;
            }
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }
        // delete detail row by id
        public async Task<ResObj> DeleteDetails(int Id)
        {
            OrderDetail orderDetail = await GetOrdersDetailByOrderDetailId(Id);
            _context.Entry(orderDetail).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        // GetOrdersDetailById only Detail
        private async Task<List<OrderDetail>> GetOrdersDetailById(int Id)
        {
            return await _context.OrderDetail.Where(i => i.OrderId == Id).ToListAsync();
        }

        // get orders detail only row
        private async Task<OrderDetail> GetOrdersDetailByOrderDetailId(int Id)
        {
            return await _context.OrderDetail.Where(i => i.OrderDetailId == Id).FirstOrDefaultAsync();
        }

        // get orders only master
        private async Task<Orders> GetOrdersById(int Id)
        {
            return await _context.Orders.Where(i => i.OrderId == Id).FirstOrDefaultAsync();

        }

        // get orders only master  for control
        public async Task<ResObj> GetById(int Id)
        {
            Orders Orders = await GetOrdersById(Id);
            return Result.Return(true, Orders);
        }

        // get all orders only master
        public async Task<ResObj> GetAll(string OrderNo, string? RestaurantName, DateTime datefrom, DateTime dateto, int? RestaurantId,int? CountriesId,int? state,
            string? phone = null, int? orderStatus = null)
        {
            List<Orders> data = await _OrdersRepository.GetEntityListAsync("dbo.GetOrderAll",
                new { OrderNo, RestaurantName, datefrom, dateto,RestaurantId , CountriesId , state });

            if (!string.IsNullOrWhiteSpace(phone))
                data = data.Where(o => o.Phone != null && o.Phone.Contains(phone.Trim())).ToList();

            if (orderStatus.HasValue)
                data = data.Where(o => MapOrderStatus(o) == orderStatus.Value).ToList();

            return Result.Return(true, data);
        }

        /// <summary>0=pending,1=approved,3=driverAccepted,4=driverEnRoute,5=pickedUp,6=outForDelivery,7=delivered,8=confirmed,9=cancelled</summary>
        public static int MapOrderStatus(Orders o)
        {
            if (o.IsCancel) return 9;
            if (o.IsDeliveryConfirmed) return 8;
            if (o.IsDelivered == true) return 7;
            if (o.IsOutForDelivery) return 6;
            if (o.IsPickedUpFromRestaurant) return 5;
            if (o.IsDriverEnRouteToPickup) return 4;
            if (o.IsSaleManApprove == true) return 3;
            if (o.IsPreparing) return 2;
            if (o.IsApporve) return 1;
            return 0;
        }

        public async Task<ResObj> GetOrdersWithDetailAll(int Id) =>
            await GetOrderFullDetails(Id);

        public async Task<ResObj> GetOrderFullDetails(int orderId)
        {
            var order = await GetOrdersById(orderId);
            if (order == null) return Result.Return(false, "الطلب غير موجود");

            List<OrderDetail> details = await _OrderDetailRepository.GetEntityListAsync("dbo.GetOrdersWithDetailAll", new { Id = orderId });

            SaleMan? driver = null;
            if (order.SaleManId is > 0)
            {
                driver = await _context.SaleMan.AsNoTracking().FirstOrDefaultAsync(s => s.SaleManId == order.SaleManId);
                var loc = await _context.DriverLocations.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.SaleManId == order.SaleManId);
                if (driver != null && loc != null)
                {
                    driver.Lat = loc.Lat.ToString(CultureInfo.InvariantCulture);
                    driver.Long = loc.Lng.ToString(CultureInfo.InvariantCulture);
                    driver.LocationUpdatedAt = loc.UpdatedAt;
                }
            }

            await EnrichOrderCoordinatesAsync(order);

            return Result.Return(true, new
            {
                order,
                details,
                driver,
                statusCode = MapOrderStatus(order)
            });
        }

        public async Task<ResObj> Post(Orders orders)
        {

            var checkshop = await _context.Restaurant.FirstOrDefaultAsync(i=>i.RestaurantId==orders.RestaurantId );
            if (checkshop == null)
            {
                return Result.Return(false,"اسم المطعم غير موجود");
            }

            var userForZone = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == orders.UserId);
            string? dropLatStr = !string.IsNullOrWhiteSpace(orders.Lat) ? orders.Lat : userForZone?.Lat;
            string? dropLngStr = !string.IsNullOrWhiteSpace(orders.Long) ? orders.Long : userForZone?.Long;
            int? customerZoneId = await ZoneCoverageHelper.ResolveZoneIdAsync(_pricing, _distance, dropLatStr, dropLngStr);
            if (!customerZoneId.HasValue)
                return Result.Return(false, "موقعك خارج مناطق التغطية");

            var restaurantZones = await ZoneCoverageHelper.GetRestaurantZoneIdsAsync(_context, orders.RestaurantId);
            if (!ZoneCoverageHelper.ServesZone(restaurantZones, customerZoneId))
                return Result.Return(false, "هذا المطعم لا يخدم منطقتك");

            var settings = await _context.AppSettings.AsNoTracking().FirstOrDefaultAsync();
            decimal minOrder = checkshop.MinimumPrice > 0
                ? checkshop.MinimumPrice
                : (settings?.DefaultOrderCost ?? 3000m);
            double itemsTotal = orders.OrderDetails?.Sum(d => d.Price * d.Count) ?? 0;
            if (orders.NetAmount > 0)
                itemsTotal = orders.NetAmount;
            else if (orders.Total > 0)
                itemsTotal = orders.Total;
            if ((decimal)itemsTotal < minOrder)
                return Result.Return(false, $"الحد الأدنى للطلب {minOrder:N0} د.ع");

            var zonesWithDrivers = await ZoneCoverageHelper.GetZonesWithAvailableDriversAsync(_context);
            if (!zonesWithDrivers.Contains(customerZoneId.Value))
                return Result.Return(false, "لا يوجد مندوب متاح في منطقتك حالياً");

            // Server-side delivery fee via pricing engine (LZA/ECA)
            double pickupLat = 0, pickupLng = 0, dropLat = 0, dropLng = 0;
            if (_distance.TryParseCoord(checkshop.Lat, checkshop.Long, out pickupLat, out pickupLng))
            {
                if (_distance.TryParseCoord(dropLatStr, dropLngStr, out dropLat, out dropLng))
                {
                    var quoteRes = await _pricing.Quote(new QuoteRequest
                    {
                        RestaurantId = orders.RestaurantId,
                        CityId = userForZone?.CityId,
                        PickupLat = pickupLat,
                        PickupLng = pickupLng,
                        DropoffLat = dropLat,
                        DropoffLng = dropLng,
                        ForceZonePricing = true
                    });
                    if (quoteRes.success && quoteRes.data is QuoteResponse quote)
                    {
                        orders.CostDelivery = quote.Total;
                        orders.PricingSource = quote.PricingSource;
                        orders.PricingFromZone = quote.FromZone;
                        orders.PricingToZone = quote.ToZone;
                        orders.RouteDistanceKm = (decimal)quote.RouteDistanceKm;
                        orders.PricingZoneFee = quote.ZoneFee;
                        orders.PricingEcaFee = quote.EcaFee;
                    }
                }
            }

            if (orders.CostDelivery == null || orders.CostDelivery < 0)
            {
                var user = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == orders.UserId);
                if (user?.CityId is > 0)
                {
                    var cityRow = await _context.RestaurantCity.AsNoTracking()
                        .FirstOrDefaultAsync(rc => rc.RestaurantId == orders.RestaurantId && rc.CityId == user.CityId);
                    if (cityRow?.CostDelivery is > 0)
                        orders.CostDelivery = cityRow.CostDelivery;
                }

                if (orders.CostDelivery == null || orders.CostDelivery < 0)
                    orders.CostDelivery = checkshop.CostDelivery ?? 3000;
            }

            var lastorder = await _context.Orders.AsSplitQuery().AsNoTracking().OrderBy(i=>i.OrderId).LastOrDefaultAsync();
            if (lastorder != null)
            {
                orders.OrderNo = lastorder.OrderNo + 1;
            }
            else orders.OrderNo = 1;
            orders.OrderDate = Key.DateTimeIQ ;
            orders.IsDone = false;
            orders.IsApporve = false;
            orders.IsCancel = false;
            orders.IsSaleManCancel = false;
            orders.IsSaleManApprove = false;
            orders.IsNotDelivered = false;
            orders.IsDelivered = false;
            orders.IsWaiting = false;
            orders.IsPreparing = false;
            orders.IsDriverEnRouteToPickup = false;
            orders.IsPickedUpFromRestaurant = false;
            orders.IsOutForDelivery = false;
            orders.IsDeliveryConfirmed = false;
            orders.Reason = "";
            orders.Reason2 = "";
            orders.SaleManId = 0;

            await _context.Orders.AddAsync(orders);
            await _context.SaveChangesAsync();

            orders.OrderDetails.ForEach(x => x.OrderId = orders.OrderId);

            await _context.OrderDetail.AddRangeAsync(orders.OrderDetails);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم حفظ الطلب بنجاح", orders);
        }

        public async Task<ResObj> GetOrdersByOrderNoAndUserId(string OrderNo, int? UserId)
        {
            List<Orders> data = await _OrdersRepository.GetEntityListAsync("dbo.GetOrdersByOrderNoAndUserId", new { OrderNo, UserId });
            return Result.Return(true, data);
        }   
        public async Task<ResObj> GetOrdersByOrderNoAndRestaurantId(string? OrderNo, int? RestaurantId,int Type)
        {                      
            List<Orders> data = await _OrdersRepository.GetEntityListAsync("dbo.GetOrdersByOrderNoAndRestaurantId", new { OrderNo, RestaurantId, Type });

            return Result.Return(true, data);
        }                                
        public async Task<ResObj> GetOrdersByOrderNoAndSaleManId(string? OrderNo, int? SaleManId, int Type)
        {                      
            List<Orders> data = await _OrdersRepository.GetEntityListAsync("dbo.GetOrdersByOrderNoAndSaleManId", new { OrderNo, SaleManId, Type });

            return Result.Return(true, data);
        }

        public async Task<ResObj> SetIsApporve(int id)
        {
            var orders = await GetOrdersById(id);
            if (orders.IsApporve)
                return Result.Return(false, "تمت الموافقة عليها سابقا");
            orders.IsApporve = true;
            orders.IsCancel = false;

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Section 6: auto-dispatch the approved order to nearby drivers (first-accept-wins).
            // Gated by Dispatch:AutoDispatchOnOrderApprove (default true).
            try
            {
                if (_config.GetValue<bool>("Dispatch:AutoDispatchOnOrderApprove", true)
                    && (orders.SaleManId == null || orders.SaleManId == 0))
                {
                    double radius = _config.GetValue<double>("Dispatch:RadiusKm", 5d);
                    var dispatchRes = await _dispatch.DispatchOrder(id, radius);
                    if (!dispatchRes.success)
                        await _logger.WriteAsync(
                            new Exception($"Dispatch failed for order {id}: {dispatchRes.msg}"),
                            "OrdersService => SetIsApporve => DispatchOrder");
                }
            }
            catch (Exception ex)
            {
                // Dispatch failure must not roll back order approval.
                await _logger.WriteAsync(ex, "OrdersService => SetIsApporve => DispatchOrder");
            }

            return Result.Return(true, "تمت الموافقة", orders);
        }

        public async Task<ResObj> SetIsPreparing(int id)
        {
            var orders = await GetOrdersById(id);
            if (!orders.IsApporve)
                return Result.Return(false, "يجب الموافقة على الطلب أولاً");
            if (orders.IsPreparing)
                return Result.Return(false, "الطلب قيد التحضير بالفعل");
            orders.IsPreparing = true;
            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "الطلب قيد التحضير", orders);
        }

        public async Task<ResObj> SetDriverEnRouteToPickup(int id)
        {
            var orders = await GetOrdersById(id);
            if (orders.SaleManId is null or 0) return Result.Return(false, "لا يوجد سائق معين");
            orders.IsDriverEnRouteToPickup = true;
            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "السائق في طريقه لاستلام طلبك", orders);
        }

        public async Task<ResObj> SetPickedUpFromRestaurant(int id)
        {
            var orders = await GetOrdersById(id);
            orders.IsPickedUpFromRestaurant = true;
            orders.IsDriverEnRouteToPickup = false;
            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم استلام الطلب من المطعم", orders);
        }

        public async Task<ResObj> SetOutForDelivery(int id)
        {
            var orders = await GetOrdersById(id);
            orders.IsOutForDelivery = true;
            orders.IsPickedUpFromRestaurant = true;
            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "جاري تسليم الطلب", orders);
        }

        public async Task<ResObj> SetDeliveryConfirmed(int id)
        {
            var orders = await GetOrdersById(id);
            var saleManId = orders.SaleManId;
            orders.IsDeliveryConfirmed = true;
            orders.IsDelivered = true;
            orders.IsOutForDelivery = false;
            orders.IsDone = true;
            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            if (saleManId is > 0)
                await _dispatch.ClearActiveOrderAsync(saleManId.Value);

            return Result.Return(true, "تم تأكيد استلام الطلب", orders);
        }

        public async Task<ResObj> SetIsSaleManApprove(int id)
        {
            var orders = await GetOrdersById(id);
             
            orders.IsSaleManApprove = true;
            orders.IsSaleManCancel = false;    

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تمت الموافقة بنجاح", orders);
        }             
        public async Task<ResObj> SetSaleManId(int id,int SaleManId)
        {
            var orders = await GetOrdersById(id);
             
            orders.SaleManId = SaleManId;      

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تمت الموافقة بنجاح", orders);
        }
                          
        public async Task<ResObj> SetIsNotDelivered(int id, string Reason)
        {
            var orders = await GetOrdersById(id);
             
            orders.IsNotDelivered = true;
            orders.IsDelivered = false;
            orders.IsWaiting = false;
            orders.Reason = Reason;     

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تمت الغاء توصيل بنجاح", orders);
        }                  
        public async Task<ResObj> SetIsWaiting(int id, string Reason2)
        {
            var orders = await GetOrdersById(id);
             
            orders.IsNotDelivered = false;
            orders.IsDelivered = false;     
            orders.IsWaiting = true;     
            orders.Reason2 = Reason2;     

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تمت تأجيل توصيل بنجاح", orders);
        }                   
        public async Task<ResObj> SetIsDelivered(int id)
        {
            var orders = await GetOrdersById(id);
             
            orders.IsNotDelivered = false;
            orders.IsDelivered = true;

            orders.IsWaiting = false;
            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تمت  توصيل  بنجاح", orders);
        }                 
        public async Task<ResObj> SetIsSaleManCancel(int id)
        {
            var orders = await GetOrdersById(id);
            var saleManId = orders.SaleManId;

            orders.IsSaleManApprove = false;
            orders.IsSaleManCancel = true;
            orders.SaleManId = 0;

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            if (saleManId is > 0)
                await _dispatch.ClearActiveOrderAsync(saleManId.Value);

            return Result.Return(true, "تمت الغاء الموافقة بنجاح", orders);
        }

        public async Task<ResObj> SetIsCancel(int id)
        {
            var orders = await GetOrdersById(id);
            if (orders.IsCancel)
                return Result.Return(false, "تمت الغاء الطلب سابقا");
            var saleManId = orders.SaleManId;
            orders.IsCancel = true;
            orders.IsApporve = false;
            orders.IsDone = false;

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            if (saleManId is > 0)
                await _dispatch.ClearActiveOrderAsync(saleManId.Value);

            return Result.Return(true, "تمت الغاء الموافقة", orders);
        }

        public async Task<ResObj> SetIsDone(int id)
        {
            var orders = await GetOrdersById(id);
            if (orders.IsDone)
                return Result.Return(false, "الطلب تمت وضعه في حالة الانتهاء");
            orders.IsDone = true;
            orders.IsApporve = true;
            orders.IsCancel = false;

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم وضع الطلب في حالة الانتهاء", orders);
        }

        public async Task<ResObj> PostUser(Users users)
        {
            if (users.UserId == 0)
            {
                await _context.AddAsync(users);
                await _context.SaveChangesAsync();
                return Result.Return(true, users);
            }
            else
            {
                var item = await _context.Users.FirstOrDefaultAsync(i=>i.UserId==users.UserId);
                if (item != null)
                {
                    //item.Name = users.Name;
                    //item.Phone = users.Phone;
                    //item.Address = users.Address;
                    //item.FunctionPoint = users.FunctionPoint;
                    item.CityId = users.CityId;
                    item.Lat = users.Lat;
                    item.Long = users.Long;        
                    _context.Entry(item).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return Result.Return(true, item);
            }
        }

        public async Task<string> GetNamePersonById(int userId)
        {
            var item= await _context.Users.Where(i => i.UserId == userId).FirstOrDefaultAsync();
            if (item == null) return "";
            else return item.Name;
        }             
        public async Task<string> GetSaleManPersonById(int SaleManId)
        {
            var item= await _context.SaleMan.Where(i => i.SaleManId == SaleManId).FirstOrDefaultAsync();
            if (item == null) return "";
            else return item.Name;
        }

        public async Task<ResObj> ModifyOrder(int orderId, List<OrderDetail> newDetails)
        {
            var order = await GetOrdersById(orderId);
            if (order == null)
                return Result.Return(false, "الطلب غير موجود");

            // Delete old details
            var oldDetails = await GetOrdersDetailById(orderId);
            foreach (var d in oldDetails)
            {
                _context.Entry(d).State = EntityState.Deleted;
            }
            await _context.SaveChangesAsync();

            // Add new details
            double total = 0;
            foreach (var d in newDetails)
            {
                d.OrderId = orderId;
                total += d.Price * d.Count;
            }
            await _context.OrderDetail.AddRangeAsync(newDetails);

            // Update order totals
            order.Total = total;
            order.NetAmount = total - order.TotalDiscount;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم تعديل الطلب بنجاح", order);
        }

        public async Task<ResObj> AddOrderDetail(OrderDetail detail)
        {
            var order = await GetOrdersById(detail.OrderId);
            if (order == null)
                return Result.Return(false, "الطلب غير موجود");

            await _context.OrderDetail.AddAsync(detail);

            // Update order totals
            order.Total += detail.Price * detail.Count;
            order.NetAmount = order.Total - order.TotalDiscount;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم اضافة المنتج بنجاح", detail);
        }

        // Section 4.1: cancel/delete the OLD order, then create & dispatch a NEW order
        // with the updated items. Returns the new order so the caller can fan out
        // notifications (merchant + driver if assigned) and re-quote pricing.
        public async Task<ResObj> AdminReplaceOrder(int orderId, List<OrderDetail> newDetails)
        {
            var oldOrder = await GetOrdersById(orderId);
            if (oldOrder == null) return Result.Return(false, "الطلب غير موجود");
            if (newDetails == null || newDetails.Count == 0)
                return Result.Return(false, "يجب اضافة منتجات للطلب الجديد");

            // 1) Cancel the old order (soft-cancel; keep the row for accounting).
            oldOrder.IsCancel = true;
            oldOrder.IsApporve = false;
            oldOrder.IsDone = false;
            oldOrder.Notes = string.IsNullOrWhiteSpace(oldOrder.Notes)
                ? "[Admin replaced this order]"
                : oldOrder.Notes + Environment.NewLine + "[Admin replaced this order]";
            _context.Entry(oldOrder).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // 2) Build the replacement.
            double total = newDetails.Sum(d => d.Price * d.Count);
            var newOrder = new Orders
            {
                RestaurantId = oldOrder.RestaurantId,
                UserId = oldOrder.UserId,
                Total = total,
                TotalDiscount = oldOrder.TotalDiscount,
                NetAmount = total - oldOrder.TotalDiscount,
                CostDelivery = oldOrder.CostDelivery,
                PromoCode = oldOrder.PromoCode,
                Notes = $"[Replacement of order #{oldOrder.OrderNo}]",
                OrderDetails = newDetails
            };
            return await Post(newOrder);
        }

        public async Task<ResObj> GetNearbyDriverOrders(int saleManId, double lat, double lng, double radiusKm)
        {
            if (!_distance.IsValidCoord(lat, lng))
                return Result.Return(false, "موقع السائق غير صالح");

            try { await _dispatch.UpdateDriverLocation(saleManId, lat, lng); } catch { }

            var driverZones = await ZoneCoverageHelper.GetDriverZoneIdsAsync(_context, saleManId);
            if (driverZones.Count == 0)
                return Result.Return(false, "لم يتم تحديد زونات العمل للمندوب — راجع الإدارة");

            var pendingOrders = await _context.Orders
                .Where(o => o.IsApporve && !o.IsDone && !o.IsCancel && (o.SaleManId == null || o.SaleManId == 0))
                .ToListAsync();

            var results = new List<NearbyDriverOrderDto>();

            foreach (var order in pendingOrders)
            {
                var restaurant = await _context.Restaurant.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RestaurantId == order.RestaurantId);
                if (restaurant == null) continue;

                if (!_distance.TryParseCoord(restaurant.Lat, restaurant.Long, out double pickupLat, out double pickupLng))
                    continue;

                var user = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == order.UserId);

                string? dropLatStr = !string.IsNullOrWhiteSpace(order.Lat) ? order.Lat : user?.Lat;
                string? dropLngStr = !string.IsNullOrWhiteSpace(order.Long) ? order.Long : user?.Long;
                if (!_distance.TryParseCoord(dropLatStr, dropLngStr, out double dropoffLat, out double dropoffLng))
                    continue;

                int? dropoffZoneId = await ZoneCoverageHelper.ResolveZoneIdAsync(_pricing, _distance, dropLatStr, dropLngStr);
                if (!ZoneCoverageHelper.ServesZone(driverZones, dropoffZoneId))
                    continue;

                double distanceToPickupKm = _distance.RoundKm(_distance.HaversineKm(lat, lng, pickupLat, pickupLng));
                if (distanceToPickupKm > radiusKm) continue;

                double pickupToDropoffKm = _distance.RoundKm(_distance.HaversineKm(pickupLat, pickupLng, dropoffLat, dropoffLng));
                double distanceToDropoffKm = _distance.RoundKm(_distance.HaversineKm(lat, lng, dropoffLat, dropoffLng));

                decimal estimatedFee = order.CostDelivery ?? 0;
                var quoteRes = await _pricing.Quote(new QuoteRequest
                {
                    RestaurantId = order.RestaurantId,
                    CityId = user?.CityId,
                    PickupLat = pickupLat,
                    PickupLng = pickupLng,
                    DropoffLat = dropoffLat,
                    DropoffLng = dropoffLng,
                    ForceZonePricing = true
                });
                if (quoteRes.success && quoteRes.data is QuoteResponse quote)
                    estimatedFee = quote.Total;

                results.Add(new NearbyDriverOrderDto
                {
                    OrderId = order.OrderId,
                    OrderNo = order.OrderNo,
                    RestaurantId = order.RestaurantId,
                    RestaurantName = restaurant.Name,
                    PickupLat = pickupLat,
                    PickupLong = pickupLng,
                    DropoffLat = dropoffLat,
                    DropoffLong = dropoffLng,
                    DistanceToPickupKm = distanceToPickupKm,
                    PickupToDropoffKm = pickupToDropoffKm,
                    DistanceToDropoffKm = distanceToDropoffKm,
                    EstimatedFee = estimatedFee,
                    UserName = user?.Name ?? order.UserName,
                    Phone = user?.Phone ?? order.Phone,
                    Address = user?.Address ?? order.Address,
                    Notes = order.Notes
                });
            }

            results.Sort((a, b) => a.DistanceToPickupKm.CompareTo(b.DistanceToPickupKm));
            return Result.Return(true, results);
        }

        private async Task EnrichOrderCoordinatesAsync(Orders order)
        {
            var restaurant = await _context.Restaurant.AsNoTracking()
                .FirstOrDefaultAsync(r => r.RestaurantId == order.RestaurantId);
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == order.UserId);

            if (_distance.TryParseCoord(restaurant?.Lat, restaurant?.Long, out double pickupLat, out double pickupLng))
            {
                order.RestaurantLat = pickupLat.ToString(CultureInfo.InvariantCulture);
                order.RestaurantLong = pickupLng.ToString(CultureInfo.InvariantCulture);
            }

            string? dropLatStr = !string.IsNullOrWhiteSpace(order.Lat) ? order.Lat : user?.Lat;
            string? dropLngStr = !string.IsNullOrWhiteSpace(order.Long) ? order.Long : user?.Long;
            if (_distance.TryParseCoord(dropLatStr, dropLngStr, out double dropoffLat, out double dropoffLng))
            {
                order.Lat = dropoffLat.ToString(CultureInfo.InvariantCulture);
                order.Long = dropoffLng.ToString(CultureInfo.InvariantCulture);
                order.DropoffLat = order.Lat;
                order.DropoffLng = order.Long;
            }
        }

        public async Task<ResObj> ApproveOrderBySaleMan(int orderId, int saleManId)
        {
            var order = await GetOrdersById(orderId);
            if (order == null)
                return Result.Return(false, "الطلب غير موجود");

            // Check if another driver already took this order
            if (order.SaleManId != null && order.SaleManId > 0 && order.SaleManId != saleManId)
                return Result.Return(false, "تم قبول الطلب من قبل مندوب آخر");

            if (order.IsSaleManApprove == true)
                return Result.Return(false, "تم قبول الطلب من قبل مندوب آخر");

            if (await _dispatch.DriverHasActiveOrderAsync(saleManId, orderId))
                return Result.Return(false, "لديك طلب نشط، أكمله قبل قبول طلب جديد");

            if (!await _dispatch.DriverServesOrderZoneAsync(saleManId, order))
                return Result.Return(false, "هذا الطلب خارج زونات عملك");

            order.SaleManId = saleManId;
            order.IsSaleManApprove = true;
            order.IsSaleManCancel = false;

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _dispatch.SetActiveOrderAsync(saleManId, orderId);

            return Result.Return(true, "تم قبول الطلب بنجاح", order);
        }
    }
}
