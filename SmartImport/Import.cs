using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Dapper;

namespace SmartImport
{
    public class Import : IDisposable
    {

        private readonly string csImportData = GetCS.importData();
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
        private string[] rr; //raw record from csv
        private List<ColumnInfo> cil;
        public List<string> tt;  //ordered type from csv
        public bool CheckFields(ReadCsv r)
        {
            rcsv = r;
            var csvColumns = new List<string>();
            int csvFieldCount = r.parser.FieldCount;
            using (IDbConnection dbConnection = ConnectionID)
            {
                //string sql = $"SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('{config.Desto}') ";
                try
                {
                    tt = new List<string>();
                    string sql = "SELECT QUOTENAME(SCHEMA_NAME(tb.[schema_id])) AS 'Schema' " +
                             ",QUOTENAME(OBJECT_NAME(tb.[OBJECT_ID])) AS 'Table' " +
                             ",C.NAME as 'Column'" +
                             ",T.name AS 'Type',C.max_length ,C.is_nullable , C.is_identity as [identity] " +
                             "FROM SYS.COLUMNS C INNER JOIN SYS.TABLES tb ON tb.[object_id] = C.[object_id] " +
                             "INNER JOIN SYS.TYPES T ON C.system_type_id = T.user_type_id " +
                             $"WHERE tb.[is_ms_shipped] = 0 and tb.[object_id] = OBJECT_ID('{config.Desto}')";



                    dbConnection.Open();
                    int tc = 0;
                    string sTempRC;
                    var data = dbConnection.Query(sql).ToList();
                    cil = new List<ColumnInfo>();
                    foreach (dynamic row in data)
                    {
                        tc++;
                        if (row.identity)
                            continue;
                        if (row.Column.IndexOf(" ") > 0)
                            sTempRC = row.Column; //.Replace("\"","");
                        else
                        {
                            sTempRC = row.Column;
                        }
                        cil.Add(new ColumnInfo{name = sTempRC, type=row.Type});
                    }
                    
                    rr = r.parser.RawRecord.Split(',');
                    for (int icnt = 0; icnt < rr.Length; icnt++)
                    {
                        tc++;
                        //Clean up carraig return in file here on column names (last one may have it).
                        if (rr[icnt].IndexOf("\n") > 0)
                        {
                            rr[icnt] = rr[icnt].Replace("\n", "");
                        }
                        if (rr[icnt].IndexOf("\r") > 0)
                        {
                            rr[icnt] = rr[icnt].Replace("\r","");
                        }
                        while (rr[icnt].IndexOf("\"") >= 0)
                        {
                            rr[icnt] = rr[icnt].Replace("\"","");
                        }
                        var lu = lookup(rr[icnt].ToLower());
                        if (lu == null)
                        {
                            var le = new LogError();
                            le.InsertError($"error field {rr[icnt]} not exist in schema {config.Name}", r.filename, config.Email,
                                false);
                            Program.Log($"error field {rr[icnt]} not exist in schema {config.Name} filename - {r.filename}");
                            return false;
                        }
                        Debug.WriteLine(" column num:" + tc);
                        tt.Add(lu.type);
                    }
                    foreach (string cs in rr)
                    {
                        var rcomp = cs;
                        csvColumns.Add(rcomp.ToLower());
                    }

                    foreach (string col in csvColumns)
                    {
                        var test = checkColumn(col.ToLower());
                        if (!test) //checks column exist in db schema
                        {
                            var le = new LogError();
                            le.InsertError($"error field {col} not exist in schema " + config.Name, r.filename,config.Email,
                                false);
                            return false;
                        }
                    }


                    return true;
                }
                catch (Exception ex)
                {
                    var le = new LogError();
                    le.InsertError(ex.Message, r.filename,config.Email,
                        false);
                    Program.Log(ex.Message + $" filename {r.filename} time:{DateTime.Now:hh:mm:ss}");
                }
            }
            return true;
        }

        private bool checkColumn(string name)
        {
            foreach (ColumnInfo ci in cil)
            {
                if (ci.name.ToLower() == name)
                    return true;
            }
            return false;
        }

        private ColumnInfo lookup(string l)
        {

            foreach (ColumnInfo ci in cil)
            {
                if (ci.name.ToLower() == l)
                    return ci;
            }
            
            if (l.IndexOf("sourcefilename") >= 0 || l.IndexOf("dateimported") >=0)
            {
                var info = new ColumnInfo();
                info.name = l;
                info.type = "varchar(max)";
                return info;
            }
            ;
            return null;
        }
        public void ImportData(Config config)
        {
            //because this is a staging table, lets first delete the records by truncating table.
            //using (IDbConnection dbConnection = ConnectionID)
            //{
            //    //string sqlT = $"truncate table {config.Desto}";
            //    //dbConnection.Execute(sqlT);
            //}
            var datastr = new StringBuilder();
            string dt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            //string qry = "SELECT top 1 * FROM ImportData." + config.Desto;
                string parms = null;
            string fieldName;
            foreach (var s in rr)
            {

                if (s.IndexOf(" ") > 0)
                {
                    fieldName = "[" + s + "]";
                }
                else
                {
                    fieldName = s;
                }
                parms += fieldName + ",";
            }
            parms += "sourcefilename,DateImported";
            //parms = parms.Remove(parms.Length - 1);
            int reccount = 0;
            using (IDbConnection dbConnection = ConnectionID)
            {
                dynamic csvr = rcsv.parser.Read();
                
                while (csvr != null && csvr.Length > 0)
                {
                    
                    int i = 0;
                    foreach (string d in csvr)
                    {
                        if (tt[i].Contains("char")  )
                        {
                            if (d.Trim().Length != 0)
                                datastr.Append( "'" + d.Replace("'", "''") + "',");
                            else
                                datastr.Append("null,");
                        }
                        else if (tt[i].Contains("date"))
                        {
                            if (d.Trim().Length < 2)
                            {
                                datastr.Append("null,");
                            }
                            else
                            {
                                DateTime ddd;
                                if (DateTime.TryParse(d, out ddd))
                                {
                                    //DateTime ddd = Convert.ToDateTime(d);
                                    if (ddd.Year < 1901)
                                        datastr.Append("null,");
                                    else
                                        datastr.Append("'" + d + "',");
                                }
                                else
                                    datastr.Append("null,");
                            }
                        }
                        else
                        {
                            if(d.Trim().Length != 0)
                                datastr.Append(d + ",");
                            else
                            {
                                datastr.Append("null,");
                            }
                        }

                        i++;
                    }
                    datastr.Append( "'" +  rcsv.filename + "','" + dt + "'" ); 
                    //data = data.Remove(data.Length - 1);
                    string sql = $"insert into {config.Desto} ({parms}) values ({datastr})";
                    datastr.Length = 0;
                    try
                    {
                        dbConnection.Execute(sql);
                    }
                    catch (Exception e)
                    {
                        var le = new LogError();
                        le.InsertError("insert error: " + e.Message, rcsv.filename, config.Email);
                        Program.Log($"error at csv line: {reccount}");
                        Program.Log($"Sql Execute Error: {sql}");
                        Program.Log($"error: {e.Message} file: {rcsv.filename}");
                    }
                    
                    reccount++;
                    csvr = rcsv.parser.Read();
                }
                string message = $"'{reccount} Records added'";
                
                
                string fnOnly = rcsv.filename.Substring(rcsv.filename.LastIndexOf("\\") + 1);
                string tempFn = $"'{fnOnly}'";
                string sqlLog = $"insert into dbo.ImportSourceLog (Daterun,Message,Filename) values (getdate(),{message},{tempFn})";
                dbConnection.Execute(sqlLog);
                Program.Log($"Completed inserting data for {rcsv.filename}, record count: {reccount}");
                rcsv.parser.Dispose();

            }
            using (IDbConnection dbConnectionService = Connection)
            {
                try
                {
                    if(config.MoveProc != null && config.MoveProc.Length > 2)
                        dbConnectionService.Execute(config.MoveProc, commandType: CommandType.StoredProcedure,commandTimeout: 500);
                }
                catch (Exception ex)
                {
                    Program.Log($"error at Stored proc {config.MoveProc}  Exception:{ex.Message}");
                    
                }
            }

        }

        private void error(string e, string second)
        {
            var err = new LogError();
            err.InsertError(e, second, config.Email);
            
        }

        public void Dispose()
        {
            rcsv?.parser.Dispose();
        }

        
    }
}
