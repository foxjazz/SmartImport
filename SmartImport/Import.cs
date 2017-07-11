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

        private readonly string csImportData = GetCS.cs();
        private readonly string connectionString = GetCS.SD();

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
                             ",T.name AS 'Type',C.max_length ,C.is_nullable , C.is_identity as [identity] " +
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

                try
                {
                    foreach (dynamic row in data)
                    {


                        if (row.identity)
                            continue;
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
                        string rowColumn = row.Column;
                        if (rowColumn.ToLower() != rcomp.ToLower())
                        {
                            var le = new LogError();
                            le.InsertError($"error field {row.Column} != {rcomp} mismatch " + config.Name, r.filename,
                                false);
                            return false;

                        }

                        i++;
                        if (i == csvFieldCount)
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    var le = new LogError();
                    le.InsertError(ex.Message, r.filename,
                        false);
                }
            }
            return true;
        }

        public void ImportData(Config config)
        {
            //because this is a staging table, lets first delete the records by truncating table.
            using (IDbConnection dbConnection = ConnectionID)
            {
                //string sqlT = $"truncate table {config.Desto}";
                //dbConnection.Execute(sqlT);
            }
            string dt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            //string qry = "SELECT top 1 * FROM ImportData." + config.Desto;
                string parms = null;
            string data;
            foreach (var s in rr)
            {
                parms += s + ",";
            }
            parms += "sourcefilename,DateImported";
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
                        {
                            if(d.Length != 0)
                                data += d + ",";
                            else
                            {
                                data += "null,";
                            }
                        }

                        i++;
                    }
                    data += "'" +  rcsv.filename + "','" + dt + "'" ; 
                    //data = data.Remove(data.Length - 1);
                    string sql = $"insert into {config.Desto} ({parms}) values ({data})";
                    try
                    {
                        dbConnection.Execute(sql);
                    }
                    catch (Exception e)
                    {
                        var le = new LogError();
                        le.InsertError("insert error: " + e.Message, rcsv.filename);
                    }
                    
                    reccount++;
                    csvr = rcsv.parser.Read();
                }
                rcsv.parser.Dispose();
                string message = $"'{reccount} Records added'";
                

                string fnOnly = rcsv.filename.Substring(rcsv.filename.LastIndexOf("\\") + 1);
              
                string tempFn = $"'{fnOnly}'";
                string sqlLog = $"insert into dbo.ImportSourceLog (Daterun,Message,Filename) values (getdate(),{message},{tempFn})";
                dbConnection.Execute(sqlLog);
               

            }
            using (IDbConnection dbConnectionService = Connection)
            {
                try
                {
                    if(config.MoveProc != null && config.MoveProc.Length > 2)
                        dbConnectionService.Execute(config.MoveProc, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    error(ex.Message, config.MoveProc);
                }
            }

        }

        private void error(string e, string second)
        {
            var err = new LogError();
            err.InsertError(e, second);
            
        }
    }
}
