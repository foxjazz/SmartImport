using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using FluentFTP;

namespace SmartImport
{
    class Program
    {
        public static DateTime lasttimeRun;
      
        static StreamWriter log;
        private static string emailAddress;
        static void Main(string[] args)
        {
            
            //First get the emailaddress used to send errors for.
            ConfigData cd = new ConfigData();
            emailAddress = cd.emailAddress;
            

            cd.PopulateTriggers();
            StringBuilder sb = new StringBuilder();
            int min;
            lasttimeRun = DateTime.Now.AddDays(-2);
            createLog();
            if (args.Length > 0 && args[0] == "export")
            {
                ExportRun();
                return;
            }
            if (args.Length > 0 && args[0] == "errorcheck")
            {
                using (var ecd = new ConfigData())
                {
                    ecd.PopulateErrorList();
                    foreach (var config in ecd.ErrorListConfig)
                    {
                        var error = new Error(config);
                        error.Process(config);
                    }
                }
                ErrorCheckRun();
                return;
            }
            try
            {
                ImportRun(args);
                cd.PopulateTriggers();
                foreach (var t in cd.importTriggers)
                {
                    Log($"run time set for: {t.ImportStart:hh:mm}");
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            try
            {
                while (true)
                {
                    if(withinImportTrigger(cd))
                    {
                        string hms = DateTime.Now.ToString("hh:mm:ss");
                        Log($"started ImportRun at {hms}");
                        ImportRun(args);
                    }
                    if (isOnHour())
                    {
                        string hms = DateTime.Now.ToString("hh:mm:ss");
                        Log($"started ExportRun at {hms}");
                        ExportRun();
                        Log($"started ErrorCheckRun at {hms}");
                        ErrorCheckRun();
                    }
                    //check every 5 minutes
                    Thread.Sleep(1000 * 60 * 5);
                    Log($"lasttimerun: {lasttimeRun:hh:mm:ss}  currenttime: {DateTime.Now:hh:mm:ss} ");
                    cd.PopulateTriggers();
                    foreach (var t in cd.importTriggers)
                    {
                        Log($"run time set for: {t.ImportStart:hh:mm}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private static int? hourDone;
        
        private static bool isOnHour()
        {
            if (!hourDone.HasValue)
            {
                //this hour hasn't been done so do it.
                hourDone = DateTime.Now.Hour;
                Log($"started export this hour is: {hourDone}");
                return true;
            }
            if (DateTime.Now.Hour != hourDone.Value)
            {
                // do it on new hours.
                hourDone = DateTime.Now.Hour;
                Log($"export is on hour: {hourDone}");
                return true;
            }
            return false;
        }
     
        private static bool withinImportTrigger(ConfigData cd)
        {
            bool valid = false;
            DateTime adjustLastrun = lasttimeRun.AddMinutes(14);
            Log($"adjustLastrun {adjustLastrun}");
            if (adjustLastrun > DateTime.Now)  //This means it was run in the last 12 minutes and the triggers should be at least 15 minutes apart.
                return false;
           
            foreach (ImportTrigger t in cd.importTriggers)
            {
                
                var dtStart = DateTime.Now.Date.AddHours(t.ImportStart.Hour).AddMinutes(t.ImportStart.Minute).AddMinutes(-6);
                var dtEnd = DateTime.Now.Date.AddHours(t.ImportStart.Hour).AddMinutes(t.ImportStart.Minute).AddMinutes(6);
                
                Log($"dtStart: {dtStart:d} time: {dtStart:t}");
                Log($"dtEnd: {dtEnd:d} time: {dtEnd:t}");
                if (DateTime.Now >= dtStart && DateTime.Now <= dtEnd)// is the time within range, if so lets run it.
                    valid = true;
            }
            return valid;
        }

        public static  void Log(string l)
        {
            log.WriteLine(l);
            Debug.WriteLine(l);
            log.Flush();
        }
        static void createLog()
        {
              
            bool exists = Directory.Exists(Directory.GetCurrentDirectory() + "\\logs");
            if (!exists)
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\logs");
            }

            string dtfn = DateTime.Now.ToString("yyyy MMMM dd hhmmss");
            string fn = "Log crpe " + dtfn + ".log";
            if (File.Exists(fn))
            {
                log = File.AppendText(Directory.GetCurrentDirectory() + "\\Logs\\" + fn);
            }
            else
            {
                log = File.CreateText(Directory.GetCurrentDirectory() + "\\Logs\\" + fn);
            }

            Log("start of log");


        }
        static void ImportRun(string[] args)
        {
            //This project moves data from csv files into the import tables listed in the ImportSource table.
            lasttimeRun = DateTime.Now;
            
            // Set time to 7:30 so if it's run in the afternoon, it will run again the next morning.
            
            Log($"Main: {lasttimeRun:hh:mm:ss}");
            //Read ImportSource table for parameters
            try
            {
                if (args.Length > 0 && args[0] == "testemail")
                {
                    var te = new LogError();
                    te.InsertError("test email", "na",emailAddress, true);
                    return;
                }
                var cd = new ConfigData();
                cd.PopulateList();
                foreach (var config in cd.ImportListConfig)
                {
                    var rcsv = new ReadCsv(config);
                    while (rcsv.Read())
                    {
                        try
                        {
                            config.Email = emailAddress;
                            var import = new Import(config);
                            if (import.CheckFields(rcsv))
                            {
                                import.ImportData(config);
                                
                            }
                            import.Dispose();
                        }
                        catch (Exception ex)
                        {
                            var n = new LogError();
                            n.InsertError(ex.Message, rcsv.filename, emailAddress);

                        }
                        try
                        {
                            if (config.archiveLocation != null &&  config.archiveLocation.Length > 2)
                            {
                                //archive already processed found files
                                
                                string target = config.archiveLocation + "\\" + Path.GetFileName(rcsv.filename);
                                if (!File.Exists(target))
                                    File.Copy(rcsv.filename, config.archiveLocation + "\\" + Path.GetFileName(rcsv.filename));
                                ReadCsv.FileDelete(rcsv.filename);
                            }

                        }
                        catch (Exception ex)
                        {
                            var n = new LogError();
                            try
                            {
                                //we have to delete to prevent endless loop.
                                ReadCsv.FileDelete(rcsv.filename);
                                Log($"delete file {rcsv.filename} time: {DateTime.Now:hh:mm:ss}");
                            }
                            catch (Exception ex2)
                            {
                                
                                n.InsertError("Delete file failed.", rcsv.filename, emailAddress);
                                Log($"delete failed {rcsv.filename} exception: {ex2.Message} time: {DateTime.Now:hh:mm:ss}");
                                throw new Exception("ERROR! Get out of loop");
                            }
                            n.InsertError("Copy to archive failed.", rcsv.filename, emailAddress);
                            Log( $"copy failed {rcsv.filename} exception: {ex.Message} time: {DateTime.Now:hh:mm:ss}");
                        }
                    }
                    

                }
            }
            catch (Exception ex)
            {
                var n = new LogError();
                n.InsertError(ex.Message, "not set yet", emailAddress);
                Log(ex.Message + $" time: {DateTime.Now:hh:mm:ss}");
            }

        }

        static bool hasRunWithin(Config c)
        {
            DateTime lastRun = c.lastRunDT;
            if (lastRun.AddMinutes(59) > DateTime.Now)
            {
                Log($"Last run + 65 min: {lastRun.AddMinutes(59)} time: {DateTime.Now}");
                return true;
            }
            return false;
        }

        static bool isNotTheHour(Config c)
        {
            if (c.scheduledHour != DateTime.Now.Hour)
                return true;
            return false;
        }

        static bool isNotDayOfMonth(Config c)
        {
            if (!c.DayOfMonth.HasValue)
                return false;
            if (c.DayOfMonth == Convert.ToInt32(DateTime.Now.ToString("MM")))
            {
                return false;
            }
            return true;

        }

        static void ErrorCheckRun()
        {
            lasttimeRun = DateTime.Now;
            try
            {
                using (var cd = new ConfigData())
                {
                    cd.PopulateErrorList();
                    foreach (var config in cd.ErrorListConfig)
                    {
                        //Here lets look at scheduledHouor and last run time to see if process should be run.
                        var error = new Error(config);
                        if (hasRunWithin(config))
                        {

                            continue;

                        }
                        if (isNotTheHour(config)) //If it's a good hoour then run bad hour then skip (continue skips)
                            continue;

                        if (isNotDayOfMonth(config))
                            continue;

                        cd.updateErrorRunDT(config);
                        //since it's time to run this lets update the RunDT so we don't run more in the same hour.
                        // For each config run the export.
                        try
                        {
                           var result =  error.Process(config);
                            if (result.Length > 0)
                            {
                                Log($"ErrorCheck listed: {config.Name} time: {DateTime.Now:hh:mm:ss}");
                            }
                        }
                        catch (Exception ex)
                        {
                            var n = new LogError();
                            n.InsertError($"Export Error on {config.Name} Exception: {ex.Message}", "not set yet", emailAddress,true);
                            Log($"Export: {config.Name} Exception{ex.Message} time: {DateTime.Now:hh:mm:ss}");

                        }
                     
                    }
                }
            }
            catch (Exception ex)
            {
                var n = new LogError();
                n.InsertError(ex.Message, "not set yet",emailAddress);
                Log(ex.Message + $" time: {DateTime.Now:hh:mm:ss}");
            }
        }

        static void ExportRun()
        {
            //This project moves data from csv files into the import tables listed in the ImportSource table.
            lasttimeRun = DateTime.Now;

            // Set time to 7:30 so if it's run in the afternoon, it will run again the next morning.
            Log($"Export Run: {lasttimeRun:hh:mm:ss}");
            //Read ImportSource table for parameters
            try
            {
                using (var cd = new ConfigData())
                {
                    cd.PopulateExportList();
                    foreach (var config in cd.ExportListConfig)
                    {
                        //Here lets look at scheduledHouor and last run time to see if process should be run.
                        var export = new Export(config);
                        if (hasRunWithin(config))
                        {

                            continue;

                        }
                        if (isNotTheHour(config)) //If it's a good hoour then run bad hour then skip (continue skips)
                            continue;

                        if (isNotDayOfMonth(config))
                            continue;

                        cd.updateExportRunDT(config);
                        //since it's time to run this lets update the RunDT so we don't run more in the same hour.
                        // For each config run the export.
                        try
                        {
                            export.Process(config);
                        }
                        catch(Exception ex)
                        {
                            var n = new LogError();
                            n.InsertError($"Export Error on {config.Name} Exception: {ex.Message}", "not set yet",emailAddress, true);
                            Log($"Export: {config.Name} Exception{ex.Message} time: {DateTime.Now:hh:mm:ss}");

                        }
                        //var fname = export.Process(config);
                        //if (string.IsNullOrEmpty(fname))  //this means no data was found no filename created.
                        //    continue;
                        //if (!string.IsNullOrEmpty(config.FtpServerName))
                        //{
                        //    FtpClient client = new FtpClient(config.FtpServerName);

                        //    // if you don't specify login credentials, we use the "anonymous" user account
                        //    client.Credentials = new NetworkCredential(config.FtpUserName, config.FtpPassword);

                        //    // begin connecting to the server
                        //    client.Connect();
                        //    client.UploadFile(config.Desto, fname);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                var n = new LogError();
                n.InsertError(ex.Message, "not set yet",emailAddress);
                Log(ex.Message + $" time: {DateTime.Now:hh:mm:ss}");
            }

        }
    }
}