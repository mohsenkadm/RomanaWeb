using RomanaWeb.Models.Entity;
using RomanaWeb.Classes;
using System.Runtime.InteropServices;

namespace RomanaWeb.Helper.Interface
{
    public interface IUsersService
    {
        Task<ResObj> RefreshToken(int Id);
        Task<ResObj> ForgatePassword(string Phone);
        Task<ResObj> ConfirmCode(string code, string Phone);
        Task<ResObj> Update_Pass_WithCode(string Pass, string Phone, string Code);

        Task<ResObj> Login(string UserName, string password);      
        Task<ResObj> GetAll(string? Name);
        Task<ResObj> Post(Users Users);
        Task<ResObj> Update(Users Users);
        Task<ResObj> Delete(int Id);
        Task<Users> GetUsersById(int Id);
        Task<ResObj> GetById(int Id);     
    }
}
