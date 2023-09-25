using RomanaWeb.Models.Entity;        
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Model.General;

namespace RomanaWeb.Helper.Repository
{
    public class AdminServices : IAdminServices, IRegisterScopped
    {
        private readonly DB_Context _context;
        private readonly IDapperRepository<Permission> _permissionService;
        private readonly IDapperRepository<Admin> _adminService;

        public AdminServices(
            DB_Context context, IDapperRepository<Permission>  dapperRepository, IDapperRepository<Admin> adminService)
        {
            _context = context;
            _permissionService = dapperRepository;
            _adminService = adminService;
        }
        public async Task<ResObj> Login(string AdminNo, string password)
        {
            password = Encyptmethod.EncryptStringToBytes_Aes(password);

            Admin? login = await _context.Admin.Where(i => i.Password == password
            && i.AdminNo == Convert.ToInt32(AdminNo)).FirstOrDefaultAsync();

            if (login is null)
                return Result.Return(false, "اسم المستخدم او كلمة المرور غير صحيحة");
                

            UserManager userManager = new UserManager() { Id = login.AdminId, Name = login.AdminName };
            login.Token = JsonWebToken.GenerateToken(userManager);

            return Result.Return(true, login);
        }
        public async Task<ResObj> GetCountForMain()
        {
            Admin admin = await _adminService.GetEntityAsync("dbo.GetCountForMain", new {  });

            return Result.Return(true, admin);
        }

        public async Task<ResObj> GetAll(string? name)
        {
            List<Admin> admin;
            if (name.IsEmpty())
                admin = await _context.Admin.AsSplitQuery().AsNoTracking().ToListAsync();
            else
                admin =await _context.Admin.AsSplitQuery().AsNoTracking().Where(i => i.AdminName.Contains(name)).ToListAsync();
            foreach (Admin Admin in admin)
            {
                if (Admin.Password != null)
                    if (Admin.Password.Length > 0)
                        Admin.Password = Encyptmethod.DecryptStringFromBytes_Aes(Admin.Password);
            }
            return Result.Return(true, admin);
        }

        public async Task<ResObj> Post(Admin Admin)
        {
            Admin.Password = Encyptmethod.EncryptStringToBytes_Aes(Admin.Password);
            await _context.Admin.AddAsync(Admin);
            await _context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح");
        }

        public async Task<ResObj> Update(Admin Admin)
        {
            Admin Admin1 = await GetAdminById(Admin.AdminId);
            if (Admin1 is null)
                return Result.Return(false, "حدث خطأ اثناء عملية جلب البيانات");          
            Admin1.AdminName = Admin.AdminName;
            Admin1.AdminNo = Admin.AdminNo; 
            Admin1.Password = Encyptmethod.EncryptStringToBytes_Aes(Admin.Password);  

            _context.Entry(Admin1).State = EntityState.Modified;
            await _context.SaveChangesAsync();
                                                               
            return Result.Return(true, "تم الحفظ بنجاح");

        }

        public async Task<ResObj> Delete(int Id)
        {
            Admin Admin1 = await GetAdminById(Id); 
            _context.Entry(Admin1).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم حذف بنجاح");
        }

        public async Task<ResObj> GetById(int Id)
        {
            Admin Admin = await GetAdminById(Id);
            if (Admin.Password.Length > 0)
            {
                Admin.Password = Encyptmethod.DecryptStringFromBytes_Aes(Admin.Password);
                Admin.Password = Admin.Password.Replace("\0", "");
            }
            return Result.Return(true, Admin);
        }

        private async Task<Admin> GetAdminById(int Id)
        {
            return await _context.Admin.AsSplitQuery().AsNoTracking().Where(i=>i.AdminId==Id).FirstOrDefaultAsync();
        }

        public async Task<ResObj> GetPermissionForLayout(int id)
        {       
            List<Permission> Permissions =  await _permissionService.GetEntityListAsync("dbo.GetPermissionForLayout", new { id }); ;
            return Result.Return(true, Permissions);
        }
        public async Task<ResObj> changestate(Permission permission)
        {
            await _permissionService.RunScriptAsync("UPDATE [dbo].[Permission]  SET  [State] ='" + permission.State + "' WHERE PermissionId=" + permission.PermissionId);
            return Result.Return(true, "تم الحفظ");
        }         
        public async Task<ResObj> GetPermissionByUserId(int UserId)
        {
            List<Permission> permissions= await _permissionService.GetEntityListAsync("dbo.GetPermissionByUserId", new { UserId });
            return Result.Return(true, permissions);
        }
    }
}
