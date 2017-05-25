using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace SmartImport
{
    public class Config
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Desto { get; set; }

        public DateTime? LastRun { get; set; }
        
        public string updateQuery { get; set; }
        public string insertQuery { get; set; }
        public string archiveLocation { get; set; }

    }
}
