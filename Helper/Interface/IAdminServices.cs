using RomanaWeb.Models.Entity;
using RomanaWeb.Classes;

namespace RomanaWeb.Helper.Interface
{
    public interface IAdminServices
    {                    
        Task<ResObj> Login(string AdminNo, string password); 
        Task<ResObj> GetCountForMain();
        Task<ResObj> GetAll(string? name);
        Task<ResObj> Post(Admin Admin);
        Task<ResObj> Update(Admin Admin);
        Task<ResObj> Delete(int Id);
        Task<ResObj> GetById(int Id);
        Task<ResObj> GetPermissionForLayout(int id);
        Task<ResObj> changestate(Permission permission);
        Task<ResObj> GetPermissionByUserId(int userId);  
    }
}
