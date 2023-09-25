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
    public class RestaurantService   : IRestaurantService,IRegisterScopped
    {
        public readonly IDapperRepository<Restaurant> _repository;
        // cotext only apply scopped 
        private readonly DB_Context _context;

        public RestaurantService(
            DB_Context context, IDapperRepository<Restaurant> repository)
        {
            _context = context;
            _repository = repository;
        }
        public async Task<ResObj> Login(string UserName, string password)
        {
            password = Encyptmethod.EncryptStringToBytes_Aes(password);

            Restaurant? login = await _context.Restaurant.Where(i => i.Password == password
            && i.UserName == UserName).FirstOrDefaultAsync();

            if (login is null)
                return Result.Return(false, "اسم المستخدم او كلمة المرور غير صحيحة");
             
             UserManager userManager = new UserManager() { Id = login.RestaurantId, Name = login.Name };
            login.Token = JsonWebToken.GenerateToken(userManager);   
            return Result.Return(true, login);
        }
        public async Task<ResObj> GetAllForApp(string? Name, int UserId, int CategoriesId)
        {
            List<Restaurant> Restaurant = await _repository.GetEntityListAsync("dbo.GetRestaurantAllForApp", new { Name, UserId, CategoriesId });
            return Result.Return(true, Restaurant);
        }     
        public async Task<ResObj> GetTopAllForApp(int? UserId, double Long, double Lat,int index)
        {
            List<Restaurant> Restaurant = await _repository.GetEntityListAsync("dbo.GetRestaurantTopAllForApp", new { UserId,Long,Lat , index });
            return Result.Return(true, Restaurant);
        }                                                          
        public async Task<ResObj> GetAll(string? Name)
        {                                                     
            List<Restaurant> Restaurant = await _repository.GetEntityListAsync("dbo.GetRestaurantAll", new { Name });
            foreach (Restaurant item in Restaurant)
            {
                if (item.Password != null)
                    if (item.Password.Length > 0)
                        item.Password = Encyptmethod.DecryptStringFromBytes_Aes(item.Password);
            }
            return Result.Return(true, Restaurant);
        }                       

        public async Task<ResObj> Post(Restaurant Restaurant)
        {                                                                              
            var checkres = await _context.Restaurant.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.Phone!.Contains(Restaurant.Phone!));
            if (checkres != null) return Result.Return(false, "رقم الهاتف موجود سابقا");
          
            Restaurant.IsClosed= false;
            Restaurant.Password= Encyptmethod.EncryptStringToBytes_Aes(Restaurant.Password!);
            await _context.Restaurant.AddAsync(Restaurant);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", Restaurant);
        }

        public async Task<ResObj> Update(Restaurant Restaurant)
        {
            Restaurant Restaurant1 = await GetRestaurantById(Restaurant.RestaurantId);
            if (Restaurant1 is null)
                return Result.Return(false, "حدث خطا اثناء عملية جلب البيانات");

            Restaurant1.Name = Restaurant.Name;
            Restaurant1.Details = Restaurant.Details;
            Restaurant1.Address = Restaurant.Address;
            if (Restaurant.Logo != null)
            if (Restaurant.Logo.Length>0)    
                    Restaurant1.Logo = Restaurant.Logo;
            if(Restaurant.Background!=null)
                if (Restaurant.Background.Length > 0)
                    Restaurant1.Background = Restaurant.Background;
            Restaurant1.Phone = Restaurant.Phone;
            Restaurant1.Lat = Restaurant.Lat;
            Restaurant1.Long = Restaurant.Long;  
            Restaurant1.Whatsapp = Restaurant.Whatsapp;  
            Restaurant1.UserName = Restaurant.UserName;   
            Restaurant1.Password = Encyptmethod.EncryptStringToBytes_Aes(Restaurant.Password!);    
            _context.Entry(Restaurant1).State = EntityState.Modified;
            await _context.SaveChangesAsync();
              UserManager userManager = new UserManager() { Id = Restaurant1.RestaurantId, Name = Restaurant.Name };
            Restaurant.Token = JsonWebToken.GenerateToken(userManager);
            return Result.Return(true, "تم الحفظ بنجاح", Restaurant1);
        }     
        public async Task UpdateCode(CodeRes code)
        {
            CodeRes? CodeRes1 = await _context.CodeRes.FirstOrDefaultAsync(i=>i.CodeResId==code.CodeResId);
            if (CodeRes1 is null)
                return;
            CodeRes1.IsFree = false;
            _context.Entry(CodeRes1).State = EntityState.Modified;
            await _context.SaveChangesAsync();  
        }



        public async Task<ResObj> Delete(int Id)
        {
            Restaurant Restaurant1 = await GetRestaurantById(Id);
            _context.Entry(Restaurant1).State = EntityState.Deleted;
               await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<Restaurant> GetRestaurantById(int Id)
        {
            var res = await _context.Restaurant.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i=>i.RestaurantId==Id);
            try { res.Password = Encyptmethod.DecryptStringFromBytes_Aes(res.Password); } catch (Exception) { }
            return res;
        }

        public async Task<ResObj> GetById(int Id)
        {
            Restaurant Restaurant = await GetRestaurantById(Id);
             return Result.Return(true, Restaurant);
        }
       
        public async Task<ResObj> SetIsColsed(int id,bool closed)
        {
            var res = await GetRestaurantById(id);         
            res.IsClosed = closed;
            res.Password = Encyptmethod.EncryptStringToBytes_Aes(res.Password);

            _context.Entry(res).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم", res);
        }
                          

        public async Task<ResObj> CheckCode(string? code)
        {
            var check = await _context.CodeRes.FirstOrDefaultAsync(i=>i.IsFree==true && i.Code==code);
            if (check != null)
            {
                return Result.Return(true, "تم",check);
            }
            else
                return Result.Return(false, "الكود مستخدم سابقا او غير متاح");
        }

        public async Task<ResObj> UpdateLocationInfo(int Id, double Long, double Lat)
        {
            var item = await _context.Restaurant.FirstOrDefaultAsync(i => i.RestaurantId == Id);
            if (item != null)
            {
                item.Long = Long.ToString();
                item.Lat = Lat.ToString();
                _context.Entry(item).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Result.Return(true, "تم ", item);
            }
            return Result.Return(false);
        }
    }
}
