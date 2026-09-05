using RomanaWeb.Models.Entity;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Model.General;

namespace RomanaWeb.Helper.Repository
{
    public class UsersService : IUsersService, IRegisterScopped
    {
        public readonly IDapperRepository<Users> _repository;
        private readonly DB_Context _context;
        private readonly IOtpService _otpService;

        public UsersService(
            DB_Context context, IDapperRepository<Users> repository, IOtpService otpService)
        {
            _context = context;
            _repository = repository;
            _otpService = otpService;
        }

        public async Task<ResObj> Login(string Phone, string password)
        {
            if (Phone.Length != 11)
            {
                return Result.Return(false, "يجب كتابة رقم الهاتف 11 رقما");
            }
            password = Encyptmethod.EncryptStringToBytes_Aes(password);

            Users? login = await _context.Users.Where(i => i.Password == password
            && i.Phone == Phone).FirstOrDefaultAsync();

            if (login is null)
                return Result.Return(false, "اسم المستخدم او كلمة المرور غير صحيحة");

            if (login.IsConfirm == false)
                return Result.Return(false, "حسابك غير مؤكد", login);
            if (login.IsActive == false)
                return Result.Return(false, "حسابك غير فعال يرجى التواصل مع مدير التطببيق");
            if (login.IsDelete == true)
                return Result.Return(false, "حسابك   محذوف يرجى التواصل مع مدير التطببيق");

            UserManager userManager = new UserManager() { Id = login.UserId, Name = login.Name, Role = "user" };
            login.Token = JsonWebToken.GenerateToken(userManager);
            login.Password = null;
            login.Code = null;
            return Result.Return(true, login);
        }

        public async Task<ResObj> LoginSendOtp(string phone)
        {
            if (phone.Length != 11)
                return Result.Return(false, "يجب كتابة رقم الهاتف 11 رقما");

            var user = await _context.Users.FirstOrDefaultAsync(i => i.Phone == phone && i.IsDelete != true);
            if (user == null)
            {
                user = new Users
                {
                    Name = phone,
                    Phone = phone,
                    Password = Encyptmethod.EncryptStringToBytes_Aes(Guid.NewGuid().ToString("N")),
                    IsConfirm = false,
                    IsActive = true,
                    IsDelete = false,
                    Code = "",
                    CityId = 0,
                    NumberSendOtp = 0,
                    OtpVerifyFailCount = 0,
                    IsBlock = false
                };
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }

            if (user.IsBlock == true)
                return Result.Return(false, "حسابك محظور، يرجى التواصل مع الادارة");
            if (user.IsActive == false)
                return Result.Return(false, "حسابك غير فعال");

            string code = OtpSettings.GenerateCode();
            return await _otpService.SendOtpToUserAsync(user, code);
        }

        public async Task<ResObj> LoginVerifyOtp(string phone, string code)
        {
            if (phone.Length != 11)
                return Result.Return(false, "يجب كتابة رقم الهاتف 11 رقما");

            if (!OtpSettings.IsValidCodeFormat(code))
                return Result.Return(false, "كود التحقق غير صحيح");

            var user = await _context.Users.FirstOrDefaultAsync(i => i.Phone == phone && i.IsDelete != true);
            if (user == null)
                return Result.Return(false, "الحساب غير موجود");
            if (user.IsBlock == true)
                return Result.Return(false, "حسابك محظور");

            var otpCheck = await ValidateAndConsumeOtpAsync(user, code);
            if (!otpCheck.success)
                return otpCheck;

            user.IsConfirm = true;
            user.NumberSendOtp = 0;
            ClearOtpFields(user);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var userManager = new UserManager { Id = user.UserId, Name = user.Name, Role = "user" };
            user.Token = JsonWebToken.GenerateToken(userManager);
            user.Password = null;
            user.Code = null;
            return Result.Return(true, "تم تسجيل الدخول بنجاح", user);
        }

        public async Task<ResObj> ForgatePassword(string Phone)
        {
            if (Phone.Length != 11)
            {
                return Result.Return(false, "يجب كتابة رقم الهاتف 11 رقما");
            }
            Users? person = await _context.Users.FirstOrDefaultAsync(i => i.Phone == Phone && i.IsDelete == false);
            if (person == null)
                return Result.Return(false, "هذا الحساب  غير موجود ");
            if (person.IsBlock == true)
                return Result.Return(false, "حسابك محظور، يرجى التواصل مع الادارة");
            if (person.IsActive == false)
                return Result.Return(false, "حسابك غير فعال");

            string code = OtpSettings.GenerateCode();
            return await _otpService.SendOtpToUserAsync(person, code);
        }

        public async Task<ResObj> Update_Pass_WithCode(string Pass, string Phone, string Code)
        {
            if (string.IsNullOrWhiteSpace(Pass) || Pass.Length < 6)
                return Result.Return(false, "كلمة المرور يجب ان تكون 6 احرف على الاقل");

            if (Phone.Length != 11)
                return Result.Return(false, "يجب كتابة رقم الهاتف 11 رقما");

            if (!OtpSettings.IsValidCodeFormat(Code))
                return Result.Return(false, "الكود غير فعال");

            Users? person = await _context.Users.FirstOrDefaultAsync(i => i.Phone == Phone && i.IsDelete == false);
            if (person == null)
                return Result.Return(false, "هذا الحساب  غير موجود ");
            if (person.IsBlock == true)
                return Result.Return(false, "حسابك محظور");

            var otpCheck = await ValidateAndConsumeOtpAsync(person, Code);
            if (!otpCheck.success)
                return otpCheck;

            person.IsConfirm = true;
            person.Password = Encyptmethod.EncryptStringToBytes_Aes(Pass);
            person.NumberSendOtp = 0;
            ClearOtpFields(person);
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
            if (!OtpSettings.IsValidCodeFormat(code))
                return Result.Return(false, "الكود غير فعال");

            Users? person = await _context.Users.FirstOrDefaultAsync(i => i.Phone == Phone && i.IsDelete != true);
            if (person == null)
                return Result.Return(false, "هذا الحساب  غير موجود ");
            if (person.IsBlock == true)
                return Result.Return(false, "حسابك محظور");

            var otpCheck = await ValidateAndConsumeOtpAsync(person, code);
            if (!otpCheck.success)
                return otpCheck;

            person.IsConfirm = true;
            ClearOtpFields(person);
            _context.Entry(person).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Do not return password/code/PII dump
            return Result.Return(true, "تم تاكيد الكود بنجاح", new
            {
                person.UserId,
                person.Name,
                person.Phone,
                person.IsConfirm
            });
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
            if (res.IsBlock == true)
                return Result.Return(false, "حسابك محظور");
            if (res.IsDelete == true)
                return Result.Return(false, "الحساب محذوف");

            UserManager userManager = new UserManager() { Id = res.UserId, Name = res.Name!, Role = "user" };
            res.Token = JsonWebToken.GenerateToken(userManager);
            res.Password = null;
            res.Code = null;
            return Result.Return(true, res);
        }

        public async Task<ResObj> GetAll(string? Name, int page = 1, int pageSize = 25)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            string? search = string.IsNullOrWhiteSpace(Name) ? null : Name.Trim();

            var query = _context.Users.AsNoTracking()
                .Where(i => i.IsDelete == false);

            if (search != null)
            {
                query = query.Where(i =>
                    (i.Name != null && i.Name.Contains(search)) ||
                    (i.Phone != null && i.Phone.Contains(search)));
            }

            int totalCount = await query.CountAsync();
            int activeCount = await query.CountAsync(i => i.IsActive == true);
            int totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page > totalPages) page = totalPages;

            var items = await query
                .OrderByDescending(i => i.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new
                {
                    i.UserId,
                    i.Name,
                    i.Phone,
                    i.Address,
                    i.FunctionPoint,
                    i.Lat,
                    i.Long,
                    i.IsConfirm,
                    i.IsActive,
                    i.IsBlock,
                    i.CityId
                })
                .ToListAsync();

            return Result.Return(true, new
            {
                items,
                totalCount,
                activeCount,
                page,
                pageSize,
                totalPages
            });
        }

        public async Task<ResObj> GetForExport(string? Name)
        {
            string? search = string.IsNullOrWhiteSpace(Name) ? null : Name.Trim();

            var query = _context.Users.AsNoTracking()
                .Where(i => i.IsDelete == false);

            if (search != null)
            {
                query = query.Where(i =>
                    (i.Name != null && i.Name.Contains(search)) ||
                    (i.Phone != null && i.Phone.Contains(search)));
            }

            // Cap export to avoid huge memory spikes
            List<Users> users = await query
                .OrderByDescending(i => i.UserId)
                .Take(10000)
                .ToListAsync();

            foreach (Users item in users)
            {
                item.Password = null;
                item.Code = null;
                item.Token = null;
            }

            return Result.Return(true, users);
        }

        public async Task<ResObj> Post(Users Users)
        {
            var checkres = await _context.Users.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.Phone!.Contains(Users.Phone!) && i.IsDelete == false);
            if (checkres != null) return Result.Return(false, "رقم الهاتف موجود سابقا");

            Users.Code = "";
            if (Users.CityId == null) Users.CityId = 0;
            // Never trust client privilege flags on self-registration.
            Users.IsConfirm = false;
            Users.IsActive = true;
            Users.IsDelete = false;
            Users.IsBlock = false;
            Users.OtpVerifyFailCount = 0;
            Users.NumberSendOtp = 0;
            Users.Password = Encyptmethod.EncryptStringToBytes_Aes(Users.Password!);
            await _context.Users.AddAsync(Users);
            await _context.SaveChangesAsync();

            UserManager userManager = new UserManager() { Id = Users.UserId, Name = Users.Name };
            Users.Token = JsonWebToken.GenerateToken(userManager);
            Users.Password = null;
            Users.Code = null;
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
            Users1.FunctionPoint = Users.FunctionPoint;
            // IsActive / IsDelete / IsConfirm / IsBlock are admin-only (PostAdmin path sets them explicitly).
            if (!string.IsNullOrWhiteSpace(Users.Password))
                Users1.Password = Encyptmethod.EncryptStringToBytes_Aes(Users.Password!);
            _context.Entry(Users1).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            UserManager userManager = new UserManager() { Id = Users1.UserId, Name = Users.Name };
            Users.Token = JsonWebToken.GenerateToken(userManager);
            Users1.Password = null;
            Users1.Code = null;
            return Result.Return(true, "تم الحفظ بنجاح", Users1);
        }

        public async Task<ResObj> UpdateAdmin(Users Users)
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
            Users1.FunctionPoint = Users.FunctionPoint;
            Users1.IsActive = Users.IsActive;
            Users1.IsDelete = Users.IsDelete;
            Users1.IsConfirm = Users.IsConfirm;
            Users1.IsBlock = Users.IsBlock;
            if (!string.IsNullOrWhiteSpace(Users.Password))
                Users1.Password = Encyptmethod.EncryptStringToBytes_Aes(Users.Password!);
            _context.Entry(Users1).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            Users1.Password = null;
            Users1.Code = null;
            return Result.Return(true, "تم الحفظ بنجاح", Users1);
        }

        public async Task<ResObj> Delete(int Id)
        {
            Users Users1 = await GetUsersById(Id);
            Users1.IsDelete = true;
            _context.Entry(Users1).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<Users> GetUsersById(int Id)
        {
            var res = await _context.Users.FirstOrDefaultAsync(i => i.UserId == Id);
            return res!;
        }

        public async Task<ResObj> GetById(int Id)
        {
            Users Users = await GetUsersById(Id);
            if (Users == null)
                return Result.Return(false, "الحساب غير موجود");
            Users.Password = null;
            Users.Code = null;
            Users.Token = null;
            return Result.Return(true, Users);
        }

        /// <summary>
        /// Validates OTP (format, match, expiry, fail limit). On failure increments fail count
        /// and persists. On success caller must ClearOtpFields + Save.
        /// </summary>
        private async Task<ResObj> ValidateAndConsumeOtpAsync(Users user, string code)
        {
            DateTime now = Key.DateTimeIQ;

            if (string.IsNullOrWhiteSpace(user.Code) || user.CodeExpiresAt == null)
                return Result.Return(false, "لا يوجد كود تحقق فعال، اطلب كوداً جديداً");

            if (now > user.CodeExpiresAt.Value)
            {
                ClearOtpFields(user);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Result.Return(false, "انتهت صلاحية كود التحقق، اطلب كوداً جديداً");
            }

            if (user.OtpVerifyFailCount >= OtpSettings.MaxVerifyFailures)
            {
                ClearOtpFields(user);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Result.Return(false, "تم تجاوز محاولات التحقق، اطلب كوداً جديداً");
            }

            if (!string.Equals(user.Code, code, StringComparison.Ordinal))
            {
                user.OtpVerifyFailCount += 1;
                if (user.OtpVerifyFailCount >= OtpSettings.MaxVerifyFailures)
                    ClearOtpFields(user);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Result.Return(false, "كود التحقق غير صحيح");
            }

            return Result.Return(true, "ok");
        }

        private static void ClearOtpFields(Users user)
        {
            user.Code = null;
            user.CodeExpiresAt = null;
            user.OtpVerifyFailCount = 0;
        }
    }
}
