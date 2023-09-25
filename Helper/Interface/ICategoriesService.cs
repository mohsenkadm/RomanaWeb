using RomanaWeb.Models.Entity;
using RomanaWeb.Classes;

namespace RomanaWeb.Helper.Interface
{
    public interface ICategoriesService
    {
        Task<ResObj> GetAll();
        Task<ResObj> Post(Categories Categories);
        Task<ResObj> Update(Categories Categories);
        Task<ResObj> Delete(int Id);
        Task<Categories> GetCategoriesById(int Id);
        Task<ResObj> GetById(int Id);
    }
}
