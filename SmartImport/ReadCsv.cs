using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Dapper;

namespace SmartImport
{
    public class ReadCsv
    {
        
        public CsvParser parser;
        public ReadCsv(string fil)
        {
            startFileName = fil.Substring(fil.LastIndexOf("\\") + 1);

            folder = fil.Substring(0, fil.LastIndexOf("\\"));

        }

        private string startFileName;
        private string folder;
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
        private List<string> getRunFiles()
        {
            using (IDbConnection dbConnection = Connection)
            {
                string sQuery = $"SELECT filename FROM ImportSourceLog where filename like '{startFileName}%'";
                dbConnection.Open();
                var s =  dbConnection.Query<Filename>(sQuery).ToList();
                var sl = new List<string>();
                foreach (var sd in s)
                {
                    sl.Add(sd.filename);
                }
                return sl;
            }

        }
        private string getFile()
        {
            //This should be dependent on files not yet run.
            List<string> runFiles = getRunFiles();
            var list = Directory.GetFiles(folder, startFileName + "*");
            foreach (var fn in list)
            {
                if (runFiles.Any(f => f.Equals(fn)))
                {
                    continue;
                }
                return fn;
            }
            return "";
        }

        public string filename { get; set; }
        public void Read()
        {


            filename = getFile();
            if (filename.Length == 0)
                return;
            try
            {
                var streamReader = new StreamReader(File.OpenRead(filename));
                parser = new CsvParser(streamReader);
                parser.Read();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
    }
}
