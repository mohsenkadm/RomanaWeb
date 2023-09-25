using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Models;
using System.Diagnostics;

namespace RomanaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAdminServices _AdminServices;
        public HomeController(IAdminServices AdminServices)
        {
            _AdminServices = AdminServices;
        }
        [HttpGet]
        public async Task<IActionResult> GetCountForMain()
        {
            try
            {
                ResObj res = await _AdminServices.GetCountForMain();


                return Json(new { success = true, data = res.data });
            }
            catch (Exception ex)
            {
                return Json(new { success = true, msg = "حدث خطأ اثناء عملية جلب البيانات" });
            }
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Admin()
        {
            return View();
        }
        public IActionResult Users()
        {
            return View();
        }
        public IActionResult Carousel()
        {
            return View();
        }
        public IActionResult Notification()
        {
            return View();
        }                     
        public IActionResult Restaurant()
        {
            return View();
        } 
        public IActionResult Categories()
        {
            return View();
        }    
        public IActionResult CodeRes()
        {
            return View();
        }             
        public IActionResult Orders()
        {
            return View();
        }             
        public IActionResult Products()
        {
            return View();
        }               
        public IActionResult PromoCode()
        {
            return View();
        }                 
        public IActionResult Stars()
        {
            return View();
        }                     
        public IActionResult RestaurantSubCategories()
        {
            return View();
        }                         
        public IActionResult SubCategories()
        {
            return View();
        }        
    }
}