using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
#if DEBUG
//config.Source = @"C:\Users\jdickinson\Documents\test";
#endif

            string fil = config.Source;

            startFileName = fil.Substring(fil.LastIndexOf("\\") + 1);

            folder = fil.Substring(0, fil.LastIndexOf("\\"));

        }

        private readonly string startFileName;
        private readonly string folder;

        private readonly string connectionString = GetCS.importData();

        public IDbConnection Connection => new SqlConnection(connectionString);

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

        public static void FileDelete(string fn)
        {
#if DEBUG
#else
            File.Delete(fn);
#endif

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
                    try
                    {
                        if (config.archiveLocation != null && config.archiveLocation.Length > 2)
                        {
                            string target = config.archiveLocation + "\\" + fnOnly;
                            if (File.Exists(target))
                                FileDelete(fn);
                            else
                            {
                                File.Copy(fn, target);
                                FileDelete(fn);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        var n = new LogError();
                        n.InsertError("Copy to archive failed." + ex.Message, fnOnly, config.Email);
                    }
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
                le.InsertError(ex.Message, "Reading File " + filename, config.Email);
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;


        }
    }
}
