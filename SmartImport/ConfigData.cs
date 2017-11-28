using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using Dapper;
namespace SmartImport
{
    public class ConfigData : IDisposable
    {
        
        public ConfigData(string n)
        {
            name = n;
        }

        public string emailAddress { get; set; }
        public void Dispose()
        {
            ImportListConfig = null;
            ExportListConfig = null;
        }
        class a
        {
            public string emailAddress { get; set; }
        }
        public ConfigData()
        {
            name = "";
           
            using (IDbConnection dbConnection = Connection)
            {
                


                List<a> listEmail;
                listEmail = dbConnection.Query<a>("select emailAddress from emailsource").ToList();
                if(listEmail.Count > 0)
                emailAddress = listEmail[0].emailAddress;
            }

        }
        public string name { get; set; }
        public List<Config> ImportListConfig { get; set; }
        public List<Config> ExportListConfig { get; set; }
        public List<Config> ErrorListConfig { get; set; }

        private readonly string connectionString = GetCS.importData();
        public IDbConnection Connection => new SqlConnection(connectionString);

        public List<ImportTrigger> importTriggers;
        public void PopulateTriggers()
        {
            using (IDbConnection dbConnection = Connection)
            {
                string sQuery;
                    sQuery = $"SELECT ImportStart from ImportTriggers";
                dbConnection.Open();
                importTriggers = dbConnection.Query<ImportTrigger>(sQuery).ToList();
              

            }
        }


        public void PopulateErrorList()
        {

            using (IDbConnection dbConnection = Connection)
            {
                string sQuery;
                if (name.Length > 0)
                {
                    sQuery = $"SELECT ID,name, Source,Email,  lastRunDT, scheduledHour FROM " +
                             $"ErrorSource" +
                             $" where name = '{name}'";
                }
                else
                {
                    sQuery = "SELECT ID,name, Source,Email,  lastRunDT, scheduledHour FROM ErrorSource where active = 1";
                }
                dbConnection.Open();
                ErrorListConfig = dbConnection.Query<Config>(sQuery).ToList();


            }


        }
        public void PopulateExportList()
        {

            using (IDbConnection dbConnection = Connection)
            {
                string sQuery;
                if (name.Length > 0)
                {
                    sQuery = $"SELECT ID,name, Source, Desto,  MoveProc, archiveLocation, lastRunDT, scheduledHour, coalesce(delimiter,',') delimiter, FtpServerName, FtpUserName, FtpPassword, FileExtension, FileName, DateFormat, DayOfMonth FROM " +
                             $"ExportSource" +
                             $" where name = '{name}'";
                }
                else
                {
                    sQuery = "SELECT ID, name, Source, Desto,  MoveProc, archiveLocation, lastRunDT, scheduledHour, coalesce(delimiter,',') delimiter, FtpServerName, FtpUserName, FtpPassword, FileExtension, FileName, DateFormat, DayOfMonth FROM ExportSource where active = 1";
                }
                dbConnection.Open();
                ExportListConfig = dbConnection.Query<Config>(sQuery).ToList();


            }


        }
        public void  PopulateList()
        {

            using (IDbConnection dbConnection = Connection)
            {
                string sQuery;
                if (name.Length > 0)
                {
                    sQuery = $"SELECT name, Source, Desto,  MoveProc, archiveLocation FROM " +
                             $"ImportSource" +
                             $" where name = '{name}'";
                }
                else
                {
                    sQuery = "SELECT name, Source, Desto,  MoveProc, archiveLocation FROM ImportSource where active = 1";
                }
                dbConnection.Open();
                    ImportListConfig =  dbConnection.Query<Config>(sQuery).ToList();
                
                
            }


        }

        public void updateExportRunDT(Config c)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string sQuery;

                sQuery = $"update ExportSource set lastRunDT = getdate() where ID = {c.ID} ";
                            
                dbConnection.Open();
                dbConnection.Execute(sQuery);


            }
        }

        public void updateErrorRunDT(Config c)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string sQuery;

                sQuery = $"update ErrorSource set lastRunDT = getdate() where ID = {c.ID} ";

                dbConnection.Open();
                dbConnection.Execute(sQuery);


            }
        }

    }


}
