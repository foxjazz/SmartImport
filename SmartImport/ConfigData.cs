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
        public ConfigData(string n)
        {
            name = n;
        }

        public ConfigData()
        {
            name = "";

        }
        public string name { get; set; }
        public List<Config> ListConfig { get; set; }

        private string connectionString = GetCS.cs();
        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(connectionString);
            }
        }

        public void  PopulateList()
        {

            using (IDbConnection dbConnection = Connection)
            {
                string sQuery;
                if (name.Length > 0)
                {
                    sQuery = $"SELECT name, Source, Desto,  MoveProc, archiveLocation FROM ImportSource where name = '{name}'";
                }
                else
                {
                    sQuery = "SELECT name, Source, Desto,  MoveProc, archiveLocation FROM ImportSource";
                }
                dbConnection.Open();
                    ListConfig =  dbConnection.Query<Config>(sQuery).ToList();
                
                
            }


        }
    }
}
