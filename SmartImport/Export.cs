using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Data;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using CsvHelper;


namespace SmartImport
{
    public class Export
    {
        private readonly string csImportData = GetCS.importData();
        private readonly string connectionString = GetCS.SD();
        private Config config;
        public Export(Config c)
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


        public string Process(Config config)
        {
            string name = config.FileName;
            DataSet ds = new DataSet("TheData");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // in export source is the stored procedure.
                SqlCommand sqlComm = new SqlCommand(config.Source, conn);
               

                sqlComm.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;

                da.Fill(ds);
              
                if (ds.Tables.Count == 0)
                {
                    return "";
                }
                string extension = "csv"; //default extensions to csv
                if (!string.IsNullOrEmpty(config.FileExtension))
                {
                    extension = config.FileExtension;
                }
                
                name += DateTime.Now.ToString(config.DateFormat) + "." +  extension;
                DataTable data = ds.Tables[0];

                using (var textWriter = File.CreateText(Path.Combine(config.Desto, name)))
                using (var csv = new CsvWriter(textWriter))
                {
                    csv.Configuration.Delimiter = config.delimiter;
                    // Write columns
                    foreach (DataColumn column in data.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    // Write row values
                    foreach (DataRow row in data.Rows)
                    {
                        for (var i = 0; i < data.Columns.Count; i++)
                        {
                            if (row[i] is DateTime)
                            {
                                DateTime dt = (DateTime)row[i];
                                csv.WriteField(dt.ToString("M/d/yyyy"));    
                            }
                            else
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
              
                
            }
            return name;
        }
    }
}
