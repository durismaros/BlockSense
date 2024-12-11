using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace BlockSense
{
    class Database
    {
        private static string _connectionString = "Server=localhost;Database=BlockSense;User=root;Password=Pa$$w0rd;";
        public static string ConnectionString { get { return _connectionString; } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<MySqlConnection> GetConnectionAsync()
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                await connection.OpenAsync();
                ConsoleHelper.WriteLine("Database connection established");
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                ConsoleHelper.WriteLine("Error: " + ex.Message);
            }

            return connection;
        }
    }
}
