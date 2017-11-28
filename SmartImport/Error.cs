using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;

namespace SmartImport
{
    public class Error
    {
        private readonly string csImportData = GetCS.importData();
        private readonly string connectionString = GetCS.SD();
        private Config config;

        public Error(Config c)
        {
            config = c;
        }

        public IDbConnection Connection
        {
            get { return new SqlConnection(connectionString); }
        }

        public IDbConnection ConnectionID
        {
            get { return new SqlConnection(csImportData); }
        }

        public string Process(Config config)
        {
            
            
            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                conn.Open();
                var data1 = conn.Query<data>(config.Source).ToList();

                if (data1[0].result > 0)
                {
                    var e = new LogError();
                    e.SendEmail(config.Email, "Error with " + config.Name, $" data returned from the following query:  \r\n {config.Source} \r\n:: run the query to find out more."+ config.Name);
                    return "error";
                }
                return "";
            }
        }

        class data
        {
            public int result { get; set; }  
        }
    }
}
