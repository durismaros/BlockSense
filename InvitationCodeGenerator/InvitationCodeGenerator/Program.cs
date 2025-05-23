using System;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Threading;

namespace InvitationCodeGenerator
{

    public class InvitationCodeGenerator
    {
        
        private readonly static string _connectionString = "Server=localhost;Database=BlockSense;User=root;Password=Pa$$w0rd;";
        private readonly static MySqlConnection _connection;

        static InvitationCodeGenerator()
        {
            _connection = new MySqlConnection(_connectionString);
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            const string header = "   _____                           _             \r\n  / ____|                         | |            \r\n | |  __  ___ _ __   ___ _ __ __ _| |_ ___  _ __ \r\n | | |_ |/ _ \\ '_ \\ / _ \\ '__/ _` | __/ _ \\| '__|\r\n | |__| |  __/ | | |  __/ | | (_| | || (_) | |   \r\n  \\_____|\\___|_| |_|\\___|_|  \\__,_|\\__\\___/|_|   \r\n                                                 \r\n                                                 ";
            int numberOfCodes;

            void RenderHeader()
            {
                Console.Clear();
                Console.WriteLine(header);
                Console.WriteLine(); // Space below the header
            }

            do
            {
                RenderHeader();
                Console.Write("How many invitation codes do you want to generate? ");
            }
    
            while (!int.TryParse(Console.ReadLine(), out numberOfCodes));

            for (int i = 0; i < numberOfCodes; i++)
            {
                string invitationCode = GenerateInvitationCode();
                InsertInvitationCode(invitationCode);
                Thread.Sleep(500);
            }

            _connection.Close();
        }

        /// <summary>
        /// Generates an invitation code
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateInvitationCode()
        {
            int length = 32;
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[length];

            for (int i = 0; i < length; i++)
                stringChars[i] = chars[random.Next(chars.Length)];

            return new string(stringChars);
        }

        /// <summary>
        /// Inserts the invitation code into the 'invitationcodes' table in BlocksenseDB
        /// </summary>
        /// <param name="code"></param>
        public static void InsertInvitationCode(string code)
        {
            try
            {
                string query = "insert into invitation_codes values (default, @code, default, default, @created_at, @expires_at, default)";
                var command = new MySqlCommand(query, _connection);
                command.Parameters.AddWithValue("@code", code);
                command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                command.Parameters.AddWithValue("@expires_at", DateTime.UtcNow.AddMonths(1));
                command.ExecuteNonQuery();

                Console.WriteLine($"Invitation code {code} added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting code into the database: " + ex.Message);
            }
        }
    }

}
