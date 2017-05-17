using System;

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