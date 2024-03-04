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

            UserManager userManager = new UserManager() { Id = login.SaleManId, Name = login.Name };
            login.Token = JsonWebToken.GenerateToken(userManager);   
            return Result.Return(true, login);
        }                                       
        public async Task<ResObj> GetCountForSale(int Id,DateTime datefrom, DateTime dateto)
        {
            SaleMan SaleMan = await _repository.GetEntityAsync("dbo.GetCountForSale", new { Id, datefrom, dateto });
            return Result.Return(true, SaleMan);
        }
        public async Task<ResObj> GetAll(string? Name)
        {                                                     
            List<SaleMan> SaleMan = await _repository.GetEntityListAsync("dbo.GetSaleManAll", new { Name });
            foreach (SaleMan item in SaleMan)
            {
                if (item.Password != null)
                    if (item.Password.Length > 0)
                        item.Password = Encyptmethod.DecryptStringFromBytes_Aes(item.Password);
            }
            return Result.Return(true, SaleMan);
        }    
        public async Task<ResObj> GetByRestaurantId(int RestaurantId)
        {                                                     
            List<SaleMan> SaleMan = await _context.SaleMan.AsSplitQuery().AsNoTracking().Where(i=>i.RestaurantId== RestaurantId).ToListAsync();
            foreach (SaleMan item in SaleMan)
            {
                if (item.Password != null)
                    if (item.Password.Length > 0)
                        item.Password = Encyptmethod.DecryptStringFromBytes_Aes(item.Password);
            }
            return Result.Return(true, SaleMan);
        }                       

        public async Task<ResObj> Post(SaleMan SaleMan)
        {                                                                              
            var checkres = await _context.SaleMan.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.Phone!.Contains(SaleMan.Phone!));
            if (checkres != null) return Result.Return(false, "رقم الهاتف موجود سابقا");
          
            
            SaleMan.Password= Encyptmethod.EncryptStringToBytes_Aes(SaleMan.Password!);
            await _context.SaleMan.AddAsync(SaleMan);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", SaleMan);
        }

        public async Task<ResObj> Update(SaleMan SaleMan)
        {
            SaleMan SaleMan1 = await GetSaleManById(SaleMan.SaleManId);
            if (SaleMan1 is null)
                return Result.Return(false, "حدث خطا اثناء عملية جلب البيانات");

            SaleMan1.Name = SaleMan.Name;
            SaleMan1.Address = SaleMan.Address;
            SaleMan1.Phone = SaleMan.Phone;
            SaleMan1.IsDelete = SaleMan.IsDelete;   
            SaleMan1.IsDelete = SaleMan.IsDelete;   
            SaleMan1.Password = Encyptmethod.EncryptStringToBytes_Aes(SaleMan.Password!);    
            _context.Entry(SaleMan1).State = EntityState.Modified;
            await _context.SaveChangesAsync();                 
            return Result.Return(true, "تم الحفظ بنجاح", SaleMan1);
        }     
        


        public async Task<ResObj> Delete(int Id)
        {
            SaleMan SaleMan1 = await GetSaleManById(Id);
            SaleMan1.IsDelete = true;
            _context.Entry(SaleMan1).State = EntityState.Modified;
               await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<SaleMan> GetSaleManById(int Id)
        {
            var res = await _repository.GetEntityAsync("dbo.GetSaleManById", new {Id}) ;
            try { res.Password = Encyptmethod.DecryptStringFromBytes_Aes(res.Password); } catch (Exception) { }
            return res;
        }

        public async Task<ResObj> GetById(int Id)
        {
            SaleMan SaleMan = await GetSaleManById(Id);
             return Result.Return(true, SaleMan);
        }
              
    }
}
