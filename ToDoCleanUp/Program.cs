using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoCleanUp
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> itemsToDelete = new List<int>();
            string connetionString = ConfigurationManager.ConnectionStrings["ToDoItemContext"].ConnectionString;
            SqlConnection connection;
            SqlCommand command;
            string sqlread = "DELETE from ToDoItem where Completed ='true'";
            SqlDataReader dataReader;
            connection = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                command = new SqlCommand(sqlread, connection);
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                }
                dataReader.Close();
                command.Dispose();

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can not open connection! Error: " + ex.Message);
            }
            Console.WriteLine("Completed");
            Console.ReadKey();
        }
    }
}
