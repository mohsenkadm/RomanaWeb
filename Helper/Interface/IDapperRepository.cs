using System.Collections.Generic;
using System.Threading.Tasks;

namespace RomanaWeb.Helper.Interface
{
    public interface IDapperRepository<TEntity>
    {
        Task<List<TEntity>> GetEntityListScriptAsync(string Query, string pars);
        Task<TEntity> GetEntityScriptAsync(string Query, string pars);
        void RunScript(string Query);
        Task RunScriptAsync(string Query);
        void RunSp(string spName, object pars);
        Task RunSpAsync(string spName, object pars);
        TEntity GetEntity(string spName, object pars);
        Task<TEntity> GetEntityAsync(string spName, object pars);
        List<TEntity> GetEntityList(string spName, object pars);
        Task<List<TEntity>> GetEntityListAsync(string spName, object pars);
    }
}
