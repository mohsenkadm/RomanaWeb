using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IAppSplashService
    {
        Task<ResObj> GetForApp();
        Task<ResObj> GetAdmin();
        Task<ResObj> Save(AppSplash splash);
    }
}
