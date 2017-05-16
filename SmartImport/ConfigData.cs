using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
namespace SmartImport
{
    public class ConfigData
    {

        public List<Config> ListConfig { get; set; }

#if DEBUG
        private const string connectionString = @"Server=SBQ201;Database=ImportData;Trusted_Connection=true;";
#else
           private const string connectionString = @"Server=87309-SB201;Database=ImportData;Trusted_Connection=true;";
#endif
        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(connectionString);
            }
        }

        public ConfigData()
        {
        }
        public void  PopulateList()
        {

            using (IDbConnection dbConnection = Connection)
            {
                
                    string sQuery = "SELECT name, Source, Desto, LastRun FROM ImportSource";
                    dbConnection.Open();
                    ListConfig =  dbConnection.Query<Config>(sQuery).ToList();
                
                
            }


        }
    }
}
