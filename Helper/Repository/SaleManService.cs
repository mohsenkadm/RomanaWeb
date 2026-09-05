using RomanaWeb.Models.Entity;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Model.General;
using RestSharp;
using Polly;
using System.Runtime.InteropServices;

namespace RomanaWeb.Helper.Repository
{
    public class SaleManService   : ISaleManService,IRegisterScopped
    {
        public readonly IDapperRepository<SaleMan> _repository;
        // cotext only apply scopped 
        private readonly DB_Context _context;

        public SaleManService(
            DB_Context context, IDapperRepository<SaleMan> repository)
        {
            _context = context;
            _repository = repository;
        }
        public async Task<ResObj> Login(string Phone, string password)
        {
            password = Encyptmethod.EncryptStringToBytes_Aes(password);

            SaleMan? login = await _context.SaleMan.Where(i => i.Password == password
            && i.Phone == Phone).FirstOrDefaultAsync();

            if (login is null)
                return Result.Return(false, "اسم المستخدم او كلمة المرور غير صحيحة");
            if (login.IsActive==false)
                return Result.Return(false, "حسابك غير فعال يرجى التواصل مع مدير التطببيق");
              if (login.IsDelete == true)
                return Result.Return(false, "حسابك   محذوف يرجى التواصل مع مدير التطببيق");

            UserManager userManager = new UserManager() { Id = login.SaleManId, Name = login.Name,Role= "sal" };
            login.Token = JsonWebToken.GenerateToken(userManager);
            login.Password = null;
            return Result.Return(true, login);
        }                                       
        public async Task<ResObj> GetCountForSale(int Id, DateTime datefrom, DateTime dateto)
        {
            var driver = await _context.SaleMan.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SaleManId == Id);
            if (driver == null)
                return Result.Return(false, "المندوب غير موجود");

            var from = datefrom.Date;
            var to = dateto.Date.AddDays(1).AddTicks(-1);
            if (to < from)
                return Result.Return(false, "تاريخ النهاية يجب ان يكون بعد تاريخ البداية");

            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.SaleManId == Id && o.OrderDate >= from && o.OrderDate <= to)
                .Select(o => new
                {
                    o.IsCancel,
                    o.IsDelivered,
                    o.IsDeliveryConfirmed,
                    o.CostDelivery,
                    o.NetAmount,
                    o.Total
                })
                .ToListAsync();

            driver.CountOrder = orders.Count;
            driver.daCountOrder = orders.Count(o => o.IsDelivered == true || o.IsDeliveryConfirmed);
            driver.NetAmount = (decimal)orders.Sum(o => o.NetAmount > 0 ? o.NetAmount : o.Total);
            driver.daNetAmount = (decimal)orders
                .Where(o => o.IsDelivered == true || o.IsDeliveryConfirmed)
                .Sum(o => o.NetAmount > 0 ? o.NetAmount : o.Total);
            driver.TotalCostDelivery = orders.Sum(o => o.CostDelivery ?? 0m);
            driver.daCostDelivery = orders
                .Where(o => o.IsDelivered == true || o.IsDeliveryConfirmed)
                .Sum(o => o.CostDelivery ?? 0m);
            driver.Total = driver.NetAmount + driver.TotalCostDelivery;
            driver.daTotal = driver.daNetAmount + driver.daCostDelivery;
            driver.Password = null;
            driver.Token = null;

            return Result.Return(true, driver);
        }
        public async Task<ResObj> GetAll(string? Name)
        {
            // EF only — exclude soft-deleted drivers (SP GetSaleManAll may return them).
            var query = _context.SaleMan.AsNoTracking()
                .Where(i => i.IsDelete == null || i.IsDelete == false);

            if (!string.IsNullOrWhiteSpace(Name))
            {
                var search = Name.Trim();
                query = query.Where(i =>
                    (i.Name != null && i.Name.Contains(search)) ||
                    (i.Phone != null && i.Phone.Contains(search)));
            }

            List<SaleMan> list = await query
                .OrderByDescending(i => i.SaleManId)
                .ToListAsync();

            foreach (SaleMan item in list)
                item.Password = null;

            return Result.Return(true, list);
        }
        public async Task<ResObj> Post(SaleMan SaleMan)
        {                                                                              
            var checkres = await _context.SaleMan.AsSplitQuery().AsNoTracking()
                .FirstOrDefaultAsync(i => i.Phone == SaleMan.Phone && (i.IsDelete == null || i.IsDelete == false));
            if (checkres != null) return Result.Return(false, "رقم الهاتف موجود سابقا");


            SaleMan.IsDelete = false;
            NormalizeMultiOrderSettings(SaleMan);
            SaleMan.Password= Encyptmethod.EncryptStringToBytes_Aes(SaleMan.Password!);
            await _context.SaleMan.AddAsync(SaleMan);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", SaleMan);
        }

        public async Task<ResObj> Update(SaleMan SaleMan)
        {
            SaleMan? SaleMan1 = await _context.SaleMan.FirstOrDefaultAsync(s => s.SaleManId == SaleMan.SaleManId);
            if (SaleMan1 is null)
                return Result.Return(false, "حدث خطا اثناء عملية جلب البيانات");

            SaleMan1.Name = SaleMan.Name;
            SaleMan1.Address = SaleMan.Address;
            SaleMan1.Phone = SaleMan.Phone;
            SaleMan1.IsDelete = false;   
            SaleMan1.IsActive = SaleMan.IsActive;
            SaleMan1.AllowMultiOrders = SaleMan.AllowMultiOrders;
            SaleMan1.MaxConcurrentOrders = SaleMan.MaxConcurrentOrders;
            NormalizeMultiOrderSettings(SaleMan1);
            if (!string.IsNullOrWhiteSpace(SaleMan.Password))
                SaleMan1.Password = Encyptmethod.EncryptStringToBytes_Aes(SaleMan.Password);
            _context.Entry(SaleMan1).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            SaleMan1.Password = null;
            return Result.Return(true, "تم الحفظ بنجاح", SaleMan1);
        }

        /// <summary>
        /// Multi off → capacity 1. Multi on → at least 2 concurrent slots (otherwise pointless).
        /// </summary>
        internal static void NormalizeMultiOrderSettings(SaleMan driver)
        {
            if (!driver.AllowMultiOrders)
            {
                if (driver.MaxConcurrentOrders < 1)
                    driver.MaxConcurrentOrders = 1;
                return;
            }

            if (driver.MaxConcurrentOrders < 2)
                driver.MaxConcurrentOrders = 2;
            if (driver.MaxConcurrentOrders > 20)
                driver.MaxConcurrentOrders = 20;
        }     
        


        public async Task<ResObj> Delete(int Id)
        {
            SaleMan? SaleMan1 = await _context.SaleMan.FirstOrDefaultAsync(i => i.SaleManId == Id);
            if (SaleMan1 is null)
                return Result.Return(false, "المندوب غير موجود");
            if (SaleMan1.IsDelete == true)
                return Result.Return(true, "تم الحذف مسبقاً");

            SaleMan1.IsDelete = true;
            SaleMan1.IsAvailable = false;
            _context.Entry(SaleMan1).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<SaleMan> GetSaleManById(int Id)
        {
            return await _context.SaleMan.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SaleManId == Id);
        }

        public async Task<ResObj> GetById(int Id)
        {
            SaleMan? SaleMan = await GetSaleManById(Id);
            if (SaleMan != null)
                SaleMan.Password = null;
             return Result.Return(true, SaleMan);
        }

        // Section 6 - flip the working/stopped flag.
        public async Task<ResObj> SetAvailability(int Id, bool isAvailable)
        {
            var driver = await _context.SaleMan.FirstOrDefaultAsync(i => i.SaleManId == Id);
            if (driver == null)
                return Result.Return(false, "المندوب غير موجود");
            if (driver.IsDelete == true)
                return Result.Return(false, "حساب المندوب محذوف");
            if (isAvailable && driver.IsActive == false)
                return Result.Return(false, "لا يمكن تفعيل حالة العمل لمندوب غير نشط — قم بتنشيطه أولاً");

            driver.IsAvailable = isAvailable;
            driver.AvailabilityChangedAt = DateTime.UtcNow;
            _context.Entry(driver).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true,
                isAvailable ? "تم تفعيل حالة العمل" : "تم ايقاف حالة العمل",
                new { driver.SaleManId, driver.IsAvailable, driver.AvailabilityChangedAt });
        }

        // نشاط المندوب: الحساب يبقى، لكن عند الإلغاء يُقفل التطبيق ولا تُجلب طلبات.
        public async Task<ResObj> SetActive(int Id, bool isActive)
        {
            var driver = await _context.SaleMan.FirstOrDefaultAsync(i => i.SaleManId == Id);
            if (driver == null)
                return Result.Return(false, "المندوب غير موجود");
            if (driver.IsDelete == true)
                return Result.Return(false, "حساب المندوب محذوف");

            driver.IsActive = isActive;
            if (!isActive)
            {
                // إيقاف حالة العمل تلقائياً حتى لا يُحسب ضمن التوزيع.
                driver.IsAvailable = false;
                driver.AvailabilityChangedAt = DateTime.UtcNow;
            }

            _context.Entry(driver).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true,
                isActive ? "تم تنشيط المندوب بنجاح" : "تم الغاء تنشيط المندوب — الحساب باقٍ لكن لا يمكنه استخدام التطبيق",
                new { driver.SaleManId, driver.IsActive, driver.IsAvailable, driver.AvailabilityChangedAt });
        }
              
    }
}
