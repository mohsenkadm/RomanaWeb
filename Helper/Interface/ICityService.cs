using RomanaWeb.Classes;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Interface
{
    public interface ICityService
    {
        public Task<ResObj> GetAll(string? Name);
        public Task<ResObj> GetByCountriesId(int CountriesId);
        public Task<ResObj> GetById(int Id);
        public Task<ResObj> Delete(int Id);
        public Task<ResObj> Post(City City);
    }
}
