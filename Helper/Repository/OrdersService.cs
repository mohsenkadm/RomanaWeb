using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;
using RomanaWeb.Model;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Helper.Interface;
using static NuGet.Packaging.PackagingConstants;

namespace RomanaWeb.Helper.Repository
{
    public class OrdersService : IOrdersService, IRegisterScopped
    {
        // cotext only apply scopped 
        private readonly DB_Context _context;
        private readonly IDapperRepository<Orders> _OrdersRepository;
        private readonly IDapperRepository<OrderDetail> _OrderDetailRepository;

        public OrdersService(
            DB_Context context,
            IDapperRepository<Orders> orderRepository,
            IDapperRepository<OrderDetail> OrderDetailRepository
            )
        {
            _context = context;
            _OrdersRepository = orderRepository;
            _OrderDetailRepository = OrderDetailRepository;
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
        public async Task<ResObj> GetAll(string OrderNo, string? RestaurantName, DateTime datefrom, DateTime dateto, int? RestaurantId,int? CountriesId,int? state)
        {
            List<Orders> data = await _OrdersRepository.GetEntityListAsync("dbo.GetOrderAll",
                new { OrderNo, RestaurantName, datefrom, dateto,RestaurantId , CountriesId , state });
            return Result.Return(true, data);
        }

        // get orders with master and detail all
        public async Task<ResObj> GetOrdersWithDetailAll(int Id)
        {
            List<OrderDetail> data = await _OrderDetailRepository.GetEntityListAsync("dbo.GetOrdersWithDetailAll",
                new { Id });
            return Result.Return(true, data);
        }

        public async Task<ResObj> Post(Orders orders)
        {

            var checkshop = await _context.Restaurant.FirstOrDefaultAsync(i=>i.RestaurantId==orders.RestaurantId );
            if (checkshop == null)
            {
                return Result.Return(false,"اسم المطعم غير موجود");
            }

            // Verify order cost / delivery cost
            if (orders.CostDelivery == null || orders.CostDelivery < 0)
            {
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
            return Result.Return(true, "تمت الموافقة", orders);
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
             
            orders.IsSaleManApprove = false;
            orders.IsSaleManCancel = true;
            orders.SaleManId = 0;

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تمت الغاء الموافقة بنجاح", orders);
        }

        public async Task<ResObj> SetIsCancel(int id)
        {
            var orders = await GetOrdersById(id);
            if (orders.IsCancel)
                return Result.Return(false, "تمت الغاء الطلب سابقا");
            orders.IsCancel = true;
            orders.IsApporve = false;
            orders.IsDone = false;

            _context.Entry(orders).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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

        public async Task<ResObj> GetNearbyDriverOrders(int saleManId, double lat, double lng, double radiusKm)
        {
            // Get pending orders (approved but no driver assigned yet)
            var pendingOrders = await _context.Orders
                .Where(o => o.IsApporve && !o.IsDone && !o.IsCancel && (o.SaleManId == null || o.SaleManId == 0))
                .ToListAsync();

            // Get restaurant locations and filter by distance
            var nearbyOrders = new List<Orders>();
            foreach (var order in pendingOrders)
            {
                var restaurant = await _context.Restaurant
                    .Where(r => r.RestaurantId == order.RestaurantId)
                    .FirstOrDefaultAsync();

                if (restaurant != null &&
                    double.TryParse(restaurant.Lat, out double rLat) &&
                    double.TryParse(restaurant.Long, out double rLng))
                {
                    double distance = CalculateDistance(lat, lng, rLat, rLng);
                    if (distance <= radiusKm)
                    {
                        order.RestaurantName = restaurant.Name;
                        order.Lat = restaurant.Lat;
                        order.Long = restaurant.Long;
                        nearbyOrders.Add(order);
                    }
                }
            }

            return Result.Return(true, nearbyOrders);
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

            order.SaleManId = saleManId;
            order.IsSaleManApprove = true;
            order.IsSaleManCancel = false;

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم قبول الطلب بنجاح", order);
        }

        /// <summary>
        /// Haversine formula to calculate distance between two GPS coordinates in km
        /// </summary>
        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
