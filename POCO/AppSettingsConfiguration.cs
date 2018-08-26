using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeagullConsulting.ContactUWP.Data.POCO
{
    public class AppSettingsConfiguration
    {
        public Database Database { get; set; }
        public Logging Logging { get; set; }
    }

    //Logging Objects
    public class Logging
    {
        public bool IncludeScopes { get; set; }
        public LogLevel LogLevel { get; set; }
    }
    public class LogLevel
    {
        public string Default { get; set; }
        public string System { get; set; }
        public string Microsoft { get; set; }
    }

    //Database Objects
    public class Database
    {
        public string Server { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string[] StoredProcedures { get; set; }
    }

}