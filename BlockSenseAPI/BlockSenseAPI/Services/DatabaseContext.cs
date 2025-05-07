using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data;

namespace BlockSenseAPI.Services
{
    public class DatabaseContext : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private MySqlConnection _connection;

        public DatabaseContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        private async Task<MySqlConnection> GetConnectionAsync()
        {
            _connection = new MySqlConnection(_connectionString);
            await _connection.OpenAsync();

            return _connection;
        }

        public async Task<DbDataReader> ExecuteReaderAsync(string query, Dictionary<string, object>? parameters = null)
        {
            var connection = await GetConnectionAsync();
            var command = new MySqlCommand(query, connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return await command.ExecuteReaderAsync();
        }

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            using (var connection = await GetConnectionAsync())
            {
                var command = new MySqlCommand(query, connection);
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return await command.ExecuteNonQueryAsync();
            }
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }
    }
}
