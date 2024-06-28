using RomanaWeb.Models.Entity;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Model.General;
using RestSharp;
using Polly;
using System.Runtime.InteropServices;
using NuGet.Protocol.Core.Types;
using OneSignalApi.Model;

namespace RomanaWeb.Helper.Repository
{
    public class UsersService   : IUsersService,IRegisterScopped
    {
        public readonly IDapperRepository<Users> _repository;
        // cotext only apply scopped 
        private readonly DB_Context _context;
        private readonly ITwilioService twilioService;

        public UsersService(
            DB_Context context, IDapperRepository<Users> repository, ITwilioService twilioService)
        {
            _context = context;
            _repository = repository;
            this.twilioService = twilioService;
        }
        public async Task<ResObj> Login(string Phone, string password)
        {
            if (Phone.Length != 11)
            {
                return Result.Return(false, "يجب كتابة رقم الهاتف 11 رقما");
            }
            password = Encyptmethod.EncryptStringToBytes_Aes(password);

            Users? login = await _context.Users.Where(i => i.Password == password
            && i.Phone == Phone ).FirstOrDefaultAsync();

            if (login is null)
                return Result.Return(false, "اسم المستخدم او كلمة المرور غير صحيحة");

            if (login.IsConfirm == false)
                return Result.Return(false, "حسابك غير مؤكد", login);
            if (login.IsActive==false)
                return Result.Return(false, "حسابك غير فعال يرجى التواصل مع مدير التطببيق");
            if (login.IsDelete == true)
                return Result.Return(false, "حسابك   محذوف يرجى التواصل مع مدير التطببيق");

            UserManager userManager = new UserManager() { Id = login.UserId, Name = login.Name ,Role="user"};
            login.Token = JsonWebToken.GenerateToken(userManager);   
            return Result.Return(true, login);
        }

        public async Task<ResObj> ForgatePassword(string Phone)
        {
            if(Phone.Length!=11)
            {
                return Result.Return(false, "يجب كتابة رقم الهاتف 11 رقما");
            }
            Users? person = await _context.Users.FirstOrDefaultAsync(i => i.Phone == Phone && i.IsDelete==false);
            if (person == null)
                return Result.Return(false, "هذا الحساب  غير موجود ");
            Random random = new Random();
            string code = random.Next(1000, 9999).ToString();          
            person.Code = code;
            _context.Entry(person).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await twilioService.SendOTPCodeToPhoneNo(Phone, code);

            return Result.Return(true, "تم ارسال كود التحقق بنجاح");
        }
        public async Task<ResObj> Update_Pass_WithCode(string Pass, string Phone, string Code)
        {
            Users? person = await _context.Users.FirstOrDefaultAsync(i => i.Phone == Phone && i.IsDelete==false);
            if (person == null)
                return Result.Return(false, "هذا الحساب  غير موجود ");
            if (Code != person.Code)
                return Result.Return(false, "الكود غير فعال");
            person.IsConfirm = true;
            person.Password = Encyptmethod.EncryptStringToBytes_Aes(Pass);
            _context.Entry(person).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم تغير كلمة المرور");
        }
        public async Task<ResObj> ConfirmCode(string code, string Phone)
        {
            if (Phone.Length != 11)
            {
                return Result.Return(false, "يجب كتابة رقم الهاتف 11 رقما");
            }
            Users? person = await _context.Users.FirstOrDefaultAsync(i => i.Phone == Phone && i.Code == code);
            if (person == null)
                return Result.Return(false, "هذا الحساب  غير موجود ");

            person.IsConfirm = true;
            _context.Entry(person).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم تاكيد الكود بنجاح", person);
        }

        public async Task<ResObj> RefreshToken(int Id)
        {
            Users? res = await _context.Users.Where(i => i.UserId == Id).FirstOrDefaultAsync();

            if (res is null)
                return Result.Return(false, "اسم المستخدم او كلمة المرور غير صحيحة");
            if (res.IsConfirm == false)
                return Result.Return(false, "حسابك غير مؤكد");
            if (res.IsActive == false)
                return Result.Return(false, "حسابك غير فعال");

             UserManager userManager = new UserManager() { Id = res.UserId, Name = res.Name!,  };
            res.Token = JsonWebToken.GenerateToken(userManager);

            return Result.Return(true, res);
        }
        public async Task<ResObj> GetAll(string? Name)
        {                                                     
            List<Users> Users = await _context.Users.AsSplitQuery().AsNoTracking().Where(i=>i.Name.Contains(Name) || Name==null  && i.IsDelete==false).ToListAsync();
            foreach (Users item in Users)
            {
                if (item.Password != null)
                    if (item.Password.Length > 0)
                        item.Password = Encyptmethod.DecryptStringFromBytes_Aes(item.Password);
            }
            return Result.Return(true, Users);
        }                       

        public async Task<ResObj> Post(Users Users)
        {                                                                              
            var checkres = await _context.Users.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.Phone!.Contains(Users.Phone!) && i.IsDelete==false);
            if (checkres != null) return Result.Return(false, "رقم الهاتف موجود سابقا");
                             
            Users.Code= "";
            if (Users.CityId == null) Users.CityId = 0;
            Users.Password= Encyptmethod.EncryptStringToBytes_Aes(Users.Password!);
            await _context.Users.AddAsync(Users);
            await _context.SaveChangesAsync();

            UserManager userManager = new UserManager() { Id = Users.UserId, Name = Users.Name };
            Users.Token = JsonWebToken.GenerateToken(userManager);
            return Result.Return(true, "تم الحفظ بنجاح يرجى تاكيد  الحساب حاليا", Users);
        }

        public async Task<ResObj> Update(Users Users)
        {
            Users Users1 = await GetUsersById(Users.UserId);
            if (Users1 is null)
                return Result.Return(false, "حدث خطا اثناء عملية جلب البيانات");

            Users1.Name = Users.Name;         
            Users1.Address = Users.Address;
            
            Users1.Phone = Users.Phone;
            Users1.Lat = Users.Lat;
            Users1.CityId = Users.CityId;
            Users1.Long = Users.Long;   
            Users1.IsActive = Users.IsActive;   
            Users1.IsDelete = Users.IsDelete;  
            Users1.Password = Encyptmethod.EncryptStringToBytes_Aes(Users.Password!);    
            _context.Entry(Users1).State = EntityState.Modified;
            await _context.SaveChangesAsync();
              UserManager userManager = new UserManager() { Id = Users1.UserId, Name = Users.Name };
            Users.Token = JsonWebToken.GenerateToken(userManager);
            return Result.Return(true, "تم الحفظ بنجاح", Users1);
        }           

        public async Task<ResObj> Delete(int Id)
        {
            Users Users1 = await GetUsersById(Id);
            Users1.IsDelete = true;
            Users1.Password = Encyptmethod.EncryptStringToBytes_Aes(Users1.Password);
            _context.Entry(Users1).State = EntityState.Modified;
               await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<Users> GetUsersById(int Id)
        {
            var res = await _context.Users.FirstOrDefaultAsync(i=>i.UserId==Id) ;
            try { res.Password = Encyptmethod.DecryptStringFromBytes_Aes(res.Password); } catch (Exception) { }
            return res;
        }

        public async Task<ResObj> GetById(int Id)
        {
            Users Users = await GetUsersById(Id);
             return Result.Return(true, Users);
        }
        
    }
}
