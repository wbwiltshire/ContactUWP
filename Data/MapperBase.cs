using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SeagullConsulting.ContactUWP.Data.Interfaces;


namespace SeagullConsulting.ContactUWP.Data
{
    public class PrimaryKey : IPrimaryKey
    {
        public object Key { get; set; }
        public bool IsIdentity { get; set; }
        public override string ToString()
        {
            return $"{Key}|{IsIdentity}";
        }
    }

    public abstract class MapToObjectBase<TEntity> : IMapToObject<TEntity>
        where TEntity : class
    {
        protected readonly ILogger logger;

        protected MapToObjectBase(ILogger l)
        {
            logger = l;
        }

        public abstract TEntity Execute(IDataReader reader);
    }
    public abstract class MapFromObjectBase<TEntity> : IMapFromObject<TEntity>
        where TEntity : class
    {
        protected readonly ILogger logger;

        protected MapFromObjectBase(ILogger l)
        {
            logger = l;
        }

        public abstract void Execute(TEntity entity, SqlCommand cmd);
    }

}
