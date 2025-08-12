using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data;

namespace BlockSenseAPI.Services
{
    public class DatabaseContext : IDisposable
    {
        private readonly MySqlConnection _connection;

        // Connection is now injected instead of created internally
        public DatabaseContext(MySqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<DbDataReader> ExecuteReaderAsync(string query, Dictionary<string, object>? parameters = null)
        {
            await EnsureConnectionOpenAsync();

            var command = CreateCommand(query, parameters);
            return await command.ExecuteReaderAsync();
        }

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            await EnsureConnectionOpenAsync();

            using (var command = CreateCommand(query, parameters))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        private MySqlCommand CreateCommand(string query, Dictionary<string, object>? parameters)
        {
            var command = new MySqlCommand(query, _connection);
            if (parameters is not null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return command;
        }

        private async Task EnsureConnectionOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        public void Dispose()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
            _connection.Dispose();
        }
    }
}
