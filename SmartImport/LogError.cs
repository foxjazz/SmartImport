using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;

namespace SmartImport
{
    public class LogError
    {

#if DEBUG
        private const string csImportData = @"Server=SBQ201;Database=ImportData;Trusted_Connection=true;";
#else
           private const string csImportData = @"Server=87309-SB201;Database=ImportData;Trusted_Connection=true;";
#endif
        public IDbConnection ConnectionID
        {
            get
            {
                return new SqlConnection(csImportData);
            }
        }
        public void InsertError(string error, string filename)
        {
            using (IDbConnection dbConnection = ConnectionID)
            {
                string tError = $"'{error}'";
                string tFilename = $"'{filename}'";
                string sql =
                    $"insert into ImportSourceErrorLog (dt,Error,Filename) values (getdate(),{tError},{tFilename})";
                dbConnection.Execute(sql);
            }
        }
    }
}
