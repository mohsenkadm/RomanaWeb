using RomanaWeb.Classes;
using System.Threading.Tasks;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface IQuestionsService
    {
        Task<ResObj> GetAllApp();
        Task<ResObj> GetAll();
        Task<ResObj> Post(Questions Questions);
        Task<ResObj> Update(Questions Questions);
        Task<ResObj> Delete(int Id);
        Task<Questions> GetQuestionsById(int Id);
        Task<ResObj> GetById(int Id);
    }
}