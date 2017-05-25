using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;

using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SmartImport
{
    public class LogError
    {

#if DEBUG
        private const string csImportData = @"Server=SBQ201;Database=ImportData;Trusted_Connection=true;";
#else
           private const string csImportData = @"Server=87309-SB201;Database=ImportData;Trusted_Connection=true;";
#endif
        public IDbConnection ConnectionID
        {
            get
            {
                return new SqlConnection(csImportData);
            }
        }
        public void InsertError(string error, string filename)
        {
            using (IDbConnection dbConnection = ConnectionID)
            {
                error = error.Replace("'", "''");
                string tError = $"'{error}'";
                string tFilename = $"'{filename}'";
                string sql =
                    $"insert into ImportSourceErrorLog (dt,Error,Filename) values (getdate(),{tError},{tFilename})";
                dbConnection.Execute(sql);
                SendEmailAsync("jdickinson@statebridgecompany.com", "SmartImport error message email", error).RunSynchronously();

            }
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Joe Bloggs", "jbloggs@example.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                client.LocalDomain = "statebridgecompany-com.mail.protection.outlook.com";
                await client.ConnectAsync("smtp.relay.uri", 25, SecureSocketOptions.None).ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}
