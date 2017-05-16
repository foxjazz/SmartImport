using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Dapper;

namespace SmartImport
{
    public class Import
    {
#if DEBUG
        private const string connectionString = @"Server=SBQ201;Database=Service;Trusted_Connection=true;";
#else
           private const string connectionString = @"Server=87309-SB201;Database=Service;Trusted_Connection=true;";
#endif

#if DEBUG
        private const string csImportData = @"Server=SBQ201;Database=ImportData;Trusted_Connection=true;";
#else
           private const string csImportData = @"Server=87309-SB201;Database=ImportData;Trusted_Connection=true;";
#endif

        private Config config;
        public Import(Config c)
        {
            config = c;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(connectionString);
            }
        }
        public IDbConnection ConnectionID
        {
            get
            {
                return new SqlConnection(csImportData);
            }
        }
        private ReadCsv rcsv;
        private string[] rr;
        public List<string> tt;
        public bool CheckFields(ReadCsv r)
        {
            rcsv = r;
            int csvFieldCount = r.parser.FieldCount;
            using (IDbConnection dbConnection = ConnectionID)
            {
                //string sql = $"SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('{config.Desto}') ";

                string sql = "SELECT QUOTENAME(SCHEMA_NAME(tb.[schema_id])) AS 'Schema' " +
                             ",QUOTENAME(OBJECT_NAME(tb.[OBJECT_ID])) AS 'Table' " +
                             ",C.NAME as 'Column'" +
                             ",T.name AS 'Type',C.max_length ,C.is_nullable " +
                             "FROM SYS.COLUMNS C INNER JOIN SYS.TABLES tb ON tb.[object_id] = C.[object_id] " +
                             "INNER JOIN SYS.TYPES T ON C.system_type_id = T.user_type_id " +
                             $"WHERE tb.[is_ms_shipped] = 0 and tb.[object_id] = OBJECT_ID('{config.Desto}')";



dbConnection.Open();
                var data = dbConnection.Query(sql).ToList();

                var x = data;
                int i=0;
                rr = r.parser.RawRecord.Split(',');
                tt = new List<string>();
                
                
                string rcomp;
                foreach (dynamic row in data)
                {
                    
                    tt.Add(row.Type);
                    rcomp = rr[i];
                    if (rcomp.IndexOf("\r") > 0)
                    {
                        rcomp = rcomp.Replace("\r", "");
                        rr[i] = rcomp;
                    }
                    if (rcomp.IndexOf("\n") > 0)
                    {
                        rcomp = rcomp.Replace("\n", "");
                        rr[i] = rcomp;
                    }
                    if (row.Column != rcomp)
                        return false;
                        
                    i++;
                    if (i == csvFieldCount)
                        return true;
                }
            }
            return true;
        }

        public void ImportData()
        {
            //string qry = "SELECT top 1 * FROM ImportData." + config.Desto;
            string parms = null;
            string data;
            foreach (var s in rr)
            {
                parms += s + ",";
            }
            parms += "sourcefilename";
            //parms = parms.Remove(parms.Length - 1);
            int reccount = 0;
            using (IDbConnection dbConnection = ConnectionID)
            {
                dynamic csvr = rcsv.parser.Read();
                
                while (csvr != null && csvr.Length > 0)
                {
                    data = null;
                    int i = 0;
                    foreach (string d in csvr)
                    {
                        if (tt[i].Contains("char") || tt[i].Contains("date"))
                        {
                            data += "'" + d.Replace("'", "''") + "',"; 
                        }
                        else
                            data += d + ",";

                        i++;
                    }
                    data += "'" +  rcsv.filename + "'" ; 
                    //data = data.Remove(data.Length - 1);
                    string sql = $"insert into {config.Desto} ({parms}) values ({data})";
                    dbConnection.Execute(sql);
                    reccount++;
                    csvr = rcsv.parser.Read();
                }
                string message = $"'{reccount} Records added'";
                string tempFn = $"'{rcsv.filename}'";
                string sqlLog =
                    $"insert into dbo.ImportSourceLog (Daterun,Message,Filename) values (getdate(),{message},{tempFn})";
                dbConnection.Execute(sqlLog);

            }
        }
    }
}
