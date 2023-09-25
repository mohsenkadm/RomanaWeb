using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;
using RomanaWeb.Model;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Helper.Interface;

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
        public async Task<ResObj> GetAll(string OrderNo, string? RestaurantName, DateTime datefrom, DateTime dateto)
        {
            List<Orders> data = await _OrdersRepository.GetEntityListAsync("dbo.GetOrderAll",
                new { OrderNo, RestaurantName, datefrom, dateto });
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

            orders.OrderDate = Key.DateTimeIQ ;
            orders.IsDone = false;
            orders.IsApporve = false;
            orders.IsCancel = false;

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
        public async Task<ResObj> GetOrdersByOrderNoAndRestaurantId(string OrderNo, int? RestaurantId)
        {                      
            List<Orders> data = await _OrdersRepository.GetEntityListAsync("dbo.GetOrdersByOrderNoAndRestaurantId", new { OrderNo, RestaurantId });

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
                return Result.Return(true, item);
            }
        }
    }
}
