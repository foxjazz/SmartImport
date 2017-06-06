using System;
using System.Collections.Generic;
using System.Text;

namespace SmartImport
{
    public class GetCS
    {
        private static  void setServer()
        {
            
            ServerName = "DBName";
        }
        private static string ServerName { get; set; }
        public static string SD()
        {
            setServer();
#if DEBUG
            return $"Server={ServerName};Database=DBName;Trusted_Connection=true;";
#else
                  return @"Server=ServerName;Database=DBName;Trusted_Connection=true;";
#endif
        }
        public static string cs()
        {
            setServer();
#if DEBUG
            return $"Server={ServerName};Database=DBName;Trusted_Connection=true;";
#else
                  return @"Server=ServerName;Database=DBName;Trusted_Connection=true;";
#endif
        }
    }
}
