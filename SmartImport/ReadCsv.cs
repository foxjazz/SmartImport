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
        private Config config;
        public ReadCsv(Config cnf)
        {
            config = cnf;
            string fil = config.Source;
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
            string fnOnly=null;
            //This should be dependent on files not yet run.
            List<string> runFiles = getRunFiles();
            var list = Directory.GetFiles(folder, startFileName + "*");
            foreach (var fn in list)
            {
                fnOnly = fn.Substring(fn.LastIndexOf("\\") + 1);
                if (runFiles.Any(f => f.Equals(fnOnly)))
                {
                    continue;
                }
                return fn;
            }
            return "";
        }

        public string filename { get; set; }
        public bool Read()
        {


            filename = getFile();
            if (filename.Length == 0)
                return false;
            try
            {
                var streamReader = new StreamReader(File.OpenRead(filename));
                parser = new CsvParser(streamReader);
                parser.Read();
                
            }
            catch (Exception ex)
            {
                var le = new LogError();
                le.InsertError(ex.Message, "Reading File " + filename);
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;


        }
    }
}
