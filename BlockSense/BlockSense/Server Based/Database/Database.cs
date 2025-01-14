using MySql.Data.MySqlClient;
using Mysqlx.Datatypes;
using Mysqlx.Prepare;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using ZstdSharp.Unsafe;

namespace BlockSense.DB
{
    class Database
    {
        protected static string _connectionString = "Server=localhost;Database=BlockSense;User=root;Password=Pa$$w0rd;";
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
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                ConsoleHelper.WriteLine("Error: " + ex.Message);
            }

            return connection;
        }

        public static async Task<DbDataReader> FetchData(string query, Dictionary<string, string> parameters)
        {
            var connection = await GetConnectionAsync();
            try
            {
                var command = new MySqlCommand(query, connection);
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
                var reader = await command.ExecuteReaderAsync();
                Console.WriteLine("Data fetched successfully");
                return reader;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public static async Task<bool> StoreData(string query, Dictionary<string, string> parameters)
        {
            using (var connection = await GetConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand(query, connection);
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine("Data stored successfully");
                    return true;

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    return false;
                }
            }
        }
    }
}
