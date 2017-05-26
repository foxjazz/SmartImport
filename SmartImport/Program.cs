using System;
using System.IO;

namespace SmartImport
{
    class Program
    {
        static void Main(string[] args)
        {
            //This project moves data from csv files into the import tables listed in the ImportSource table.

            //Read ImportSource table for parameters
            try
            {
                if (args.Length > 0 && args[0] == "testemail")
                {
                    var te = new LogError();
                    te.InsertError("test email", "na", true);
                    return;
                }
                var cd = new ConfigData();
                cd.PopulateList();
                foreach (var config in cd.ListConfig)
                {
                    var rcsv = new ReadCsv(config);
                    while (rcsv.Read())
                    {
                        try
                        {
                            var import = new Import(config);
                            if (import.CheckFields(rcsv))
                            {
                                import.ImportData(config);
                            }
                        }
                        catch (Exception ex)
                        {
                            var n = new LogError();
                            n.InsertError(ex.Message, rcsv.filename);

                        }
                        try
                        {
                            if (config.archiveLocation != null &&  config.archiveLocation.Length > 2)
                            {
                                //archive already processed found files
                                string fullSource = config.Source + "\\" + rcsv.filename;
                                string target = config.archiveLocation + "\\" + rcsv.filename;
                                if (!File.Exists(target))
                                    File.Copy(rcsv.filename, config.archiveLocation + "\\" + rcsv.filename);
                                ReadCsv.FileDelete(rcsv.filename);
                            }

                        }
                        catch
                        {
                            var n = new LogError();
                            n.InsertError("Copy to archive failed.", rcsv.filename);
                        }
                    }
                    

                }
            }
            catch (Exception ex)
            {
                var n = new LogError();
                n.InsertError(ex.Message, "not set yet");
            }

        }
    }
}