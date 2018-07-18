using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToDoNotifications
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> EmailsToSend = new List<string>();
            string connetionString = ConfigurationManager.ConnectionStrings["ToDoItemContext"].ConnectionString;
            SqlConnection connection;
            SqlCommand command;
            string sqlread = "select Email from UserProfile Where UserName IN (SELECT Owner FROM ToDoItem Where DateDeadline < GETDATE() Group by [Owner])  and EnableNotifications = '1'";
            SqlDataReader dataReader;
            connection = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                command = new SqlCommand(sqlread, connection);
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    EmailsToSend.Add(dataReader.GetString(0));
                }
                dataReader.Close();
                command.Dispose();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can not open connection! Error: " + ex.Message);
            }

            try
            {
                foreach (var email in EmailsToSend)
                {
                    MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["SMTPFrom"], email);
                    SmtpClient client = new SmtpClient();
                    client.Port = int.Parse(ConfigurationManager.AppSettings["SMTPPort"]);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Host = ConfigurationManager.AppSettings["SMTPServer"];
                    client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPUser"], ConfigurationManager.AppSettings["SMTPPassword"]);
                    mail.Subject = "Notification from ToDo";
                    mail.Body = "You have tasks, that are expired. Check those out at our amazing website!";
                    client.Send(mail);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            Console.WriteLine("Completed");
            Thread.Sleep(5000);
        }
    }
}
