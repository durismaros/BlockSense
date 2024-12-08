using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Reflection.Metadata.Ecma335;
using ZstdSharp.Unsafe;

namespace BlockSense
{
    class Database
    {
        private static string connectionString = "Server=localhost;Database=BlockSense;User=root;Password=Pa$$w0rd;";
        public static string ConnectionString { get { return connectionString; } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                Console.WriteLine("Database connection established");
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                Console.WriteLine("Error: " + ex.Message);
            }

            return connection;
        }
    }
}
