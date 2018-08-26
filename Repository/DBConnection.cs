using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using SeagullConsulting.ContactUWP.Data.Interfaces;

namespace SeagullConsulting.ContactUWP.Data.Repository
{
    public class DBConnection : IDBConnection
    {
        private ILogger logger;
        private SqlConnection connection = null;
        private bool isOpen = false;

        //ctor
        public DBConnection(string connectionString, ILogger l)
        {
            logger = l;
            isOpen = false;
            try
            {
                connection = new SqlConnection(connectionString);
                logger.LogInformation($"ConnectionString: {connectionString}");

            }
            catch (SqlException ex)
            {
                logger.LogError(ex.Message);
            }
        }

        public SqlConnection Connection
        {
            get { return connection; }
        }

        public async Task<bool> Open()
        {
            //Only create a connection, if we don't already have one
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    await connection.OpenAsync();
                    isOpen = true;
                    logger.LogInformation("DBConnection opened.");
                }
                catch (SqlException ex)
                {
                    logger.LogError(ex.Message);
                }
            }
            else
                logger.LogInformation("Using open DBConnection.");

            return isOpen;
        }

        public void Close()
        {
            try
            {
                if (connection != null)
                {
                    connection.Close();
                    isOpen = false;
                    logger.LogInformation("DBConnection closed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        #region Dispose
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (connection != null)
                        connection.Close();
                    logger.LogInformation("DBConnection closed via dispose.");
                }
                this.disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}