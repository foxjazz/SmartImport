using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SmartImport
{
    public class DBConnection
    {
        public string serviceConnstr;
        public string ImportDataConnstr;
        public string StatebridgeConnstr;
        public DBConnection()
        {
            SetConnections();
        }

        public DBConnection(string OverRide)
        {
            SetConnections();

        }

        private void SetConnections()
        {
            if (Environment.MachineName.IndexOf("prod") > 0)
            {
                serviceConnstr = serviceDev;
            }
            else if (Environment.MachineName.IndexOf("qa") > 0)
            {
                serviceConnstr = serviceQA;
            }
            else if (Environment.MachineName.IndexOf("dev") > 0)
            {
                serviceConnstr = serviceProd;
            }
            else
            {
                serviceConnstr = serviceDev;
            }
        }

        private readonly string serviceProd  = @"Server=prod;Database=dbName;Trusted_Connection=true;";
        private readonly string serviceQA = @"Server=qa;Database=dbName;Trusted_Connection=true;";
        private readonly string serviceDev = @"Server=dev;Database=dbName;Trusted_Connection=true;";

    }
}
