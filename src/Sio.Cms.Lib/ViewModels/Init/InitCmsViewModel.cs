using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sio.Cms.Lib.ViewModels.SioInit
{
    public class InitCmsViewModel
    {
        #region Properties
        [JsonProperty("connectionString")]
        public string ConnectionString
        {
            // If use local db  => return local db cnn string
            // Else If use remote db 
            // => return: if use mysql => return mysql cnn string
            //              else return remote mssql cnn string
            get
            {
                switch (DatabaseProvider)
                {
                    case SioEnums.DatabaseProvider.MSSQL:
                        return IsUseLocal
                    ? LocalDbConnectionString

                    : $"Server={DataBaseServer};Database={DataBaseName}" +
                    $";UID={DataBaseUser};Pwd={DataBasePassword};MultipleActiveResultSets=true;"
                    ;
                    case SioEnums.DatabaseProvider.MySQL:
                        return $"Server={DataBaseServer};Database={DataBaseName}" +
                      $";User={DataBaseUser};Password={DataBasePassword};";

                    default:
                        return string.Empty;
                }

            }
        }

        [JsonProperty("dataBaseServer")]
        public string DataBaseServer { get; set; }

        [JsonProperty("dataBaseName")]
        public string DataBaseName { get; set; }

        [JsonProperty("dataBaseUser")]
        public string DataBaseUser { get; set; }

        [JsonProperty("dataBasePassword")]
        public string DataBasePassword { get; set; }

        [JsonProperty("isUseLocal")]
        public bool IsUseLocal { get; set; }

        [JsonProperty("localDbConnectionString")]
        public string LocalDbConnectionString { get; set; } =
            $"Server=(localdb)\\MSSQLLocalDB;Initial Catalog=sio-cms.db;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True";

        [JsonProperty("sqliteDbConnectionString")]
        public string SqliteDbConnectionString { get; set; } = $"Data Source=sio-cms.db";

        [JsonProperty("superAdminsuperAdmin")]
        public string SuperAdmin { get; set; }

        [JsonProperty("adminPassword")]
        public string AdminPassword { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }

        [JsonProperty("isMysql")]
        public bool IsMysql { get; set; }

        [JsonProperty("databaseProvider")]
        public SioEnums.DatabaseProvider DatabaseProvider { get; set; }

        [JsonProperty("culture")]
        public InitCulture Culture { get; set; }

        [JsonProperty("siteName")]
        public string SiteName { get; set; } = "SioCore";
        #endregion

        public InitCmsViewModel()
        {

        }


    }
}
