using System;
using System.Collections.Generic;
using System.Text;

namespace SmartImport
{
    public class Config
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string Desto { get; set; }

        public DateTime? LastRun { get; set; }
        
        public string MoveProc { get; set; }
        
        public string archiveLocation { get; set; }

        public DateTime lastRunDT
        {
            get; set;
            
        }
        public int scheduledHour { get; set; }
        public string delimiter { get; set; }
        public string FtpServerName { get; set; }
        public string FtpUserName { get; set; }
        public string FtpPassword { get; set; }
        public string FileExtension { get; set; }
        public string FileName { get; set; }
        public string DateFormat { get; set; }
        public int? DayOfMonth { get; set; }
        public string Email { get; set; }
    }
}
