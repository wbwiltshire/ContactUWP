using System;
using System.Collections.Generic;
//using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeagullConsulting.ContactUWP.Data;
using SeagullConsulting.ContactUWP.Data.Interfaces;
using SeagullConsulting.ContactUWP.Data.POCO;

namespace SeagullConsulting.ContactUWP.Data.Repository
{
    public abstract class RepositoryBase<TEntity>
          where TEntity : class
    {
        private UnitOfWork unitOfWork;
        private readonly ILogger logger;
        private DBConnection dbc;

        #region ctor
        //ctor with no unit of work necessary
        protected RepositoryBase(AppSettingsConfiguration s, ILogger l, DBConnection d)
        {
            Settings = s;
            logger = l;
            dbc = d;
        }
        //ctor with unit of work
        protected RepositoryBase(AppSettingsConfiguration s, ILogger l, UnitOfWork uow, DBConnection d)
        {
            Settings = s;
            logger = l;
            unitOfWork = uow;
            dbc = d;
        }
        #endregion

        public string OrderBy { get; set; }
        protected string CMDText { get; set; }
        protected AppSettingsConfiguration Settings { get; private set; }
        protected Constants.DBCommandType SqlCommandType { get; set; }
        //protected MapperBase<TEntity> Mapper { get; set; }
        protected MapToObjectBase<TEntity> MapToObject { get; set; }
        protected MapFromObjectBase<TEntity> MapFromObject { get; set; }

        protected async Task<int> FindAllCount()
        {
            object cnt;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    await dbc.Open();

                using (SqlCommand cmd = new SqlCommand(CMDText, dbc.Connection))
                {
                    //Returns an object, not an int
                    cnt = await cmd.ExecuteScalarAsync();
                    logger.LogInformation("FindAllCount complete.");
                    if (cnt != null)
                        return Convert.ToInt32(cnt);
                    else
                        return 0;
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
                return 0;
            }
        }

        public virtual async Task<ICollection<TEntity>> FindAll()
        {
            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    await dbc.Open();

                using (SqlCommand cmd = new SqlCommand(CMDText, dbc.Connection))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        ICollection<TEntity> entities = new List<TEntity>();
                        while (await reader.ReadAsync())
                        {
                            entities.Add(MapToObject.Execute(reader));
                        }
                        logger.LogInformation($"FindAll complete for {typeof(TEntity)} entity.");
                        return entities;
                    }
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }

        public virtual async Task<ICollection<TEntity>> FindAllPaged(int offset, int pageSize)
        {
            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    await dbc.Open();

                using (SqlCommand cmd = new SqlCommand(CMDText, dbc.Connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@p1", offset));
                    cmd.Parameters.Add(new SqlParameter("@p2", pageSize));

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        ICollection<TEntity> entities = new List<TEntity>();
                        while (await reader.ReadAsync())
                        {
                            entities.Add(MapToObject.Execute(reader));
                        }
                        logger.LogInformation($"FindAllPaged complete for {typeof(TEntity)} entity.");
                        return entities;
                    }
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }

        public virtual async Task<TEntity> FindByPK(IPrimaryKey pk)
        {
            TEntity entity = null;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    await dbc.Open();

                using (SqlCommand cmd = new SqlCommand(CMDText, dbc.Connection))
                {
                    cmd.Parameters.Add(new SqlParameter("@pk", pk.Key));
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                            entity = MapToObject.Execute(reader);
                        else
                            entity = null;
                        logger.LogInformation($"FindByPK complete for {typeof(TEntity)} entity.");
                    }
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
                return null;
            }

            return entity;
        }

        protected async Task<object> Add(TEntity entity, PrimaryKey pk)
        {
            object result = null;
            int rows = 0;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    await dbc.Open();

                if (unitOfWork != null) await unitOfWork.Enlist();
                using (SqlCommand cmd = new SqlCommand(CMDText, dbc.Connection))
                {
                    if (SqlCommandType == Constants.DBCommandType.SPROC)
                        cmd.CommandType = CommandType.StoredProcedure;
                    MapFromObject.Execute(entity, cmd);

                    //If Identity, then it's numeric
                    if (pk.IsIdentity)
                    {
                        //returns PK
                        result = await cmd.ExecuteScalarAsync();
                    }
                    else
                    {
                        //returns rows updated and sets result to key
                        cmd.Parameters.Add(new SqlParameter("@pk", pk.Key));
                        rows = await cmd.ExecuteNonQueryAsync();
                        result = pk.Key;
                    }
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
            }
            logger.LogInformation($"Add complete for {typeof(TEntity)} entity.");
            return result;
        }

        protected async Task<int> Update(TEntity entity, IPrimaryKey pk)
        {
            int rows = 0;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    await dbc.Open();

                if (unitOfWork != null) await unitOfWork.Enlist();
                using (SqlCommand cmd = new SqlCommand(CMDText, dbc.Connection))
                {
                    if (SqlCommandType == Constants.DBCommandType.SPROC)
                        cmd.CommandType = CommandType.StoredProcedure;
                    MapFromObject.Execute(entity, cmd);
                    cmd.Parameters.Add(new SqlParameter("@pk", pk.Key));
                    rows = await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
            }
            logger.LogInformation($"Update complete for {typeof(TEntity)} entity.");
            return rows;
        }

        protected async Task<int> Delete(IPrimaryKey pk)
        {
            int rows = 0;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    await dbc.Open();

                if (unitOfWork != null) await unitOfWork.Enlist();
                using (SqlCommand cmd = new SqlCommand(CMDText, dbc.Connection))
                {
                    cmd.Parameters.Add(new SqlParameter("@pk", pk.Key));
                    rows = await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
            }
            logger.LogInformation($"Delete complete for {typeof(TEntity)} entity.");
            return rows;
        }


        #region ExecNonQuery
        protected async Task<int> ExecNonQuery(IList<SqlParameter> p)
        {
            int rows = 0;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    await dbc.Open();

                using (SqlCommand cmd = new SqlCommand(CMDText, dbc.Connection))
                {
                    if (SqlCommandType == Constants.DBCommandType.SPROC)
                        cmd.CommandType = CommandType.StoredProcedure;

                    foreach (SqlParameter s in p)
                        cmd.Parameters.Add(s);

                    //Returns an object, not an int
                    rows = await cmd.ExecuteNonQueryAsync();
                    logger.LogInformation("ExecNonQuery complete.");
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
                rows = 0;
            }

            return rows;
        }
        #endregion

        #region ExecStoredProc
        protected async Task<int> ExecStoredProc(IList<SqlParameter> p)
        {
            int cnt = 0;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    await dbc.Open();


                using (SqlCommand cmd = new SqlCommand(CMDText, dbc.Connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter s in p)
                        cmd.Parameters.Add(s);

                    cnt = await cmd.ExecuteNonQueryAsync();
                    logger.LogInformation("ExecStoredProc complete.");
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
                return 0;
            }
            return cnt;
        }
        #endregion

    }
}
