using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServer
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.Title = "Сервер авторизации";
            Console.ForegroundColor = ConsoleColor.Green;
            Gateway Gateway = new Gateway(Settings1.Default.SRV_PORT);
            try
            {
               
                Console.WriteLine($"SQL Connected! Version: {new MySqlCommand("SELECT @@VERSION;", Gateway.GetPolledConnection()).ExecuteScalar()}");
                Console.ResetColor();
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error connection to SQL Server! {e.Message}");
                Console.ResetColor();
            }
            Thread.Sleep(-1);
        }
    }
}
