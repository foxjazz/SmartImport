using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace SmartImport
{
    public class LogError
    {

        public LogError()
        {
            this.sendMail = false;
        }
        private readonly string csImportData = GetCS.cs();
        public IDbConnection ConnectionID
        {
            get
            {
                return new SqlConnection(csImportData);
            }
        }

        public bool sendMail { get; set; }
        public void InsertError(string error, string filename, bool sm)
        {
            this.sendMail = sm;
            this.InsertError(error,filename);
        }
        public void InsertError(string error, string filename)
        {
            using (IDbConnection dbConnection = ConnectionID)
            {
                error = error.Replace("'", "''");
                string tError = $"'{error}'";
                string tFilename = $"'{filename}'";
                string sql =
                    $"insert into ImportSourceErrorLog (dt,Error,Filename) values (getdate(),{tError},{tFilename})";
                dbConnection.Execute(sql);

                if(sendMail)
                    SendEmail("jdickinson@statebridgecompany.com", "SmartImport error message email", error);

            }
        }
        public  void SendEmail(string email, string subject, string message)
        {
            using (IDbConnection dbConnectionService = ConnectionID)
            {
                var t = new SqlParameter();
                t.DbType = System.Data.DbType.String;
                DynamicParameters dp = new DynamicParameters();
                t.ParameterName = "@sendto";
                t.SqlValue = "jdickinson@statebridgecompany.com";
                dp.Add(t.ParameterName, t.SqlValue, t.DbType);

                t.ParameterName = "@sub";
                t.SqlValue = subject;
                dp.Add(t.ParameterName, t.SqlValue, t.DbType);
                t.ParameterName = "@bod";
                t.SqlValue = message;
                dp.Add(t.ParameterName, t.SqlValue, t.DbType);

                try
                {
                    dbConnectionService.Execute("SendMail", dp, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
