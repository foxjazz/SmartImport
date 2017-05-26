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

        private string connectionString = GetCS.cs();
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
                
                    string sQuery = "SELECT name, Source, Desto,  MoveProc, archiveLocation FROM ImportSource";
                    dbConnection.Open();
                    ListConfig =  dbConnection.Query<Config>(sQuery).ToList();
                
                
            }


        }
    }
}
