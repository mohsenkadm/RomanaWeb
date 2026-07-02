using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface ISupportPhoneService
    {
        Task<ResObj> GetAll();
        Task<ResObj> GetById(int id);
        Task<ResObj> GetForApp(int appType);
        Task<ResObj> GetAllForApps();
        Task<ResObj> Post(SupportPhone supportPhone);
        Task<ResObj> Delete(int id);
    }
}
