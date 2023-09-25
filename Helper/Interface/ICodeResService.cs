using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface ICodeResService
    {
        public Task<ResObj> GetAll();                    
        public Task<ResObj> GetById(int Id);
        public Task<ResObj> Delete(int Id);
        public Task<ResObj> Post(CodeRes CodeRes);
    }
}
