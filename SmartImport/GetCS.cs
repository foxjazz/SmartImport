using System;
using System.Collections.Generic;
using System.Text;

namespace SmartImport
{
    public class GetCS
    {
        private static  void setServer()
        {
            //ServerName = "SBD201";
            ServerName = "87309-SB201";
        }
        private static string ServerName { get; set; }
        public static string SD()
        {
            setServer();
#if DEBUG
            return $"Server={ServerName};Database=Service;Trusted_Connection=true;";
#else
                  return @"Server=87309-SB201;Database=Service;Trusted_Connection=true;";
#endif
        }
        public static string cs()
        {
            setServer();
#if DEBUG
            return $"Server={ServerName};Database=ImportData;Trusted_Connection=true;";
#else
                  return @"Server=87309-SB201;Database=ImportData;Trusted_Connection=true;";
#endif
        }
    }
}
