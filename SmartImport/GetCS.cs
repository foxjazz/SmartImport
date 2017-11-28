using System;
using System.Collections.Generic;
using System.Text;

namespace SmartImport
{
    public class GetCS
    {
       
        private static string ServerName { get; set; }
        private static string ProdServerName { get; set; }
        public static string SD()
        {
            var conn = new DBConnection();
            return conn.serviceConnstr;
//            setServer();
//#if DEBUG
//            return $"Server={ServerName};Database=Service;Trusted_Connection=true;";
//#else
//            return $"Server={ProdServerName};Database=Service;Trusted_Connection=true;";
            
//#endif
        }
        public static string importData()
        {
            var conn = new DBConnection();
            return conn.ImportDataConnstr;
//            setServer();
//#if DEBUG
//            return $"Server={ServerName};Database=ImportData;Trusted_Connection=true;";
//#else
//            return $"Server={ProdServerName};Database=ImportData;Trusted_Connection=true;";
            
//#endif
        }
    }
}
