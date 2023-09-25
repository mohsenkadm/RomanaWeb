using RomanaWeb.Helper.Interface;
using Microsoft.Data.SqlClient;
using System;
using System.Data.Common;
using Dapper;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using RomanaWeb.Classes;

namespace RomanaWeb.Helper.Repository
{
    public class DapperRepository<TEntity> : IDapperRepository<TEntity>, IRegisterSingleton
    {
        private static DbConnection Connection;

        public DapperRepository()
        {
            Connection = new SqlConnection(DBConn.ConnectionString);

            try
            {
                Connection.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// This Func use to execute Get Entity List command
        /// </summary>
        /// <param name="Query">SQL query</param>
        public async Task<List<TEntity>> GetEntityListScriptAsync(string Query, string pars)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    await Connection.OpenAsync();

                IEnumerable<TEntity> result = await Connection.QueryAsync<TEntity>(File.ReadAllText(Query) + pars, commandType: CommandType.Text);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// This Func use to execute Get Entity command
        /// </summary>
        /// <param name="Query">SQL query</param>
        public async Task<TEntity> GetEntityScriptAsync(string Query, string pars)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    await Connection.OpenAsync();
                IEnumerable<TEntity> result = await Connection.QueryAsync<TEntity>(File.ReadAllText(Query) + pars, commandType: CommandType.Text);
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// This Func use to execute Insert, Update and Delete command
        /// </summary>
        /// <param name="Query">SQL query</param>
        public void RunScript(string Query)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    Connection.Open();

                Connection.Query(Query, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// This Func use to execute Insert, Update and Delete command
        /// </summary>
        /// <param name="Query">SQL query</param>
        public async Task RunScriptAsync(string Query)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    await Connection.OpenAsync();

                await Connection.QueryAsync(Query, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// This Func use to execute stored procedure that not return any data 
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <param name="pars">object of parameters</param>
        public void RunSp(string spName, object pars)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    Connection.OpenAsync();

                Connection.Query(spName, pars, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// This Func use to execute stored procedure that not return any data 
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <param name="pars">object of parameters</param>
        public async Task RunSpAsync(string spName, object pars)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    await Connection.OpenAsync();

                await Connection.QueryAsync(spName, pars, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// This Func use to execute stored procedure that return single object
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <param name="pars">object of parameters</param>
        /// <returns>TEntity</returns>
        public TEntity GetEntity(string spName, object pars)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    Connection.Open();

                IEnumerable<TEntity> result = Connection.Query<TEntity>(spName, pars,
                   commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// This Func use to execute stored procedure that return single object
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <param name="pars">object of parameters</param>
        /// <returns>TEntity</returns>
        public async Task<TEntity> GetEntityAsync(string spName, object pars)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    await Connection.OpenAsync();

                IEnumerable<TEntity> result = await Connection.QueryAsync<TEntity>(spName, pars,
                   commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// This Func use to execute stored procedure that return list of object
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <param name="pars">object of parameters</param>
        /// <returns>List of TEntity</returns>
        public List<TEntity> GetEntityList(string spName, object pars)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    Connection.Open();

                IEnumerable<TEntity> result = Connection.Query<TEntity>(spName, pars,
                   commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// This Func use to execute stored procedure that return list of object
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <param name="pars">object of parameters</param>
        /// <returns>List of TEntity</returns>
        public async Task<List<TEntity>> GetEntityListAsync(string spName, object pars)
        {
            try
            {
                if (Connection == null && Connection.State == ConnectionState.Closed)
                    await Connection.OpenAsync();

                IEnumerable<TEntity> result = await Connection.QueryAsync<TEntity>(spName, pars,
                   commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
