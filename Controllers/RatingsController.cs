using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Controllers
{
    // Section 5.1: customer rating endpoints (after order completion).
    // Routes match the spec: POST /orders/{id}/rating/store, POST /orders/{id}/rating/driver
    [Authorize]
    [Route("orders/{id:int}/rating")]
    public class RatingsController : MasterController
    {
        private readonly ILoggerRepository _logger;
        private readonly IStarsService _starsService;
        private readonly IDriverStarsService _driverStarsService;
        private readonly DB_Context _context;

        public RatingsController(
            ILoggerRepository logger,
            IStarsService starsService,
            IDriverStarsService driverStarsService,
            DB_Context context)
        {
            _logger = logger;
            _starsService = starsService;
            _driverStarsService = driverStarsService;
            _context = context;
        }

        public class RatingRequest
        {
            public int Stars { get; set; }
            public string? Comment { get; set; }
        }

        [HttpPost("store")]
        public async Task<IActionResult> RateStore(int id, [FromBody] RatingRequest req)
        {
            try
            {
                if (req == null || req.Stars < 1 || req.Stars > 5)
                    return Response(false, "stars must be between 1 and 5");

                var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == id);
                if (order == null) return Response(false, "الطلب غير موجود");
                // Section 5.1: only after status = COMPLETED (IsDone == true).
                if (!order.IsDone) return Response(false, "لا يمكن تقييم الطلب قبل اكتماله");

                var existing = await _context.Stars.AsNoTracking().FirstOrDefaultAsync(s => s.OrderId == id);
                if (existing != null) return Response(false, "تم تقييم الطلب مسبقا");

                var rating = new Stars
                {
                    StarsCount = req.Stars,
                    RestaurantId = order.RestaurantId,
                    Comments = req.Comment ?? string.Empty,
                    OrderId = id,
                    UserId = order.UserId
                };
                await _starsService.Post(rating);
                return Response(true, "تم حفظ التقييم");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RatingsController => RateStore");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }

        [HttpPost("driver")]
        public async Task<IActionResult> RateDriver(int id, [FromBody] RatingRequest req)
        {
            try
            {
                if (req == null || req.Stars < 1 || req.Stars > 5)
                    return Response(false, "stars must be between 1 and 5");

                var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == id);
                if (order == null) return Response(false, "الطلب غير موجود");
                if (!order.IsDone) return Response(false, "لا يمكن تقييم الطلب قبل اكتماله");
                if (order.SaleManId == null || order.SaleManId == 0)
                    return Response(false, "لا يوجد سائق مرتبط بالطلب");

                var existing = await _context.DriverStars.AsNoTracking().FirstOrDefaultAsync(s => s.OrderId == id);
                if (existing != null) return Response(false, "تم تقييم السائق مسبقا");

                var rating = new DriverStars
                {
                    StarsCount = req.Stars,
                    SaleManId = order.SaleManId,
                    Comments = req.Comment ?? string.Empty,
                    OrderId = id
                };
                await _driverStarsService.Post(rating);
                return Response(true, "تم حفظ التقييم");
            }
            catch (Exception ex)
            {
                await _logger.WriteAsync(ex, "RatingsController => RateDriver");
                return Response(false, "حدث خطأ اثناء عملية الحفظ");
            }
        }
    }
}
