using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace BlockSenseAPI.Services
{
    /// <summary>
    /// Represents a database context for interacting with MySQL database.
    /// </summary>
    public class DatabaseContext : IDisposable
    {
        private readonly MySqlConnection _connection;
        public MySqlConnection Connection => _connection;

        /// <summary>
        /// Initializes a new instance of <see cref="DatabaseContext"/> class with the provided MySqlConnection.
        /// </summary>
        /// <param name="connection">The MySQL connection to use. Must not be null</param>
        /// <exception cref="ArgumentNullException">Thrown if connection is null.</exception>
        public DatabaseContext(MySqlConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// Ensures that the database connection is open.
        /// </summary>
        /// <returns></returns>
        private async Task EnsureConnectionOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
        }

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <param name="isolationLevel">Specifies the transaction isolation level. Defaults to ReadCommitted.</param>
        /// <returns>A new <see cref="MySqlTransaction"/> instance with desired isolation level.</returns>
        public async Task<MySqlTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            await EnsureConnectionOpenAsync();
            return _connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Executes a query that returns multiple rows.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional dictionary of parameters (name-value pairs).</param>
        /// <param name="transaction">Optional transaction to execute the command in.</param>
        /// <returns>A <see cref="DbDataReader"/> for reading the result.</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(string query, Dictionary<string, object>? parameters = null, MySqlTransaction? transaction = null)
        {
            await EnsureConnectionOpenAsync();
            var cmd = CreateCommand(query, parameters);
            if (transaction != null)
                cmd.Transaction = transaction;

            // Close connection automatically when no active transaction
            var behavior = (transaction == null) ? CommandBehavior.CloseConnection : CommandBehavior.Default;
            return await cmd.ExecuteReaderAsync(behavior);
        }

        /// <summary>
        /// Executes a query that returns a single scalar value.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional dictionary of parameters (name-value pairs).</param>
        /// <param name="transaction">Optional transaction to execute the command in.</param>
        /// <returns>The first column of the first row in the result.</returns>
        public async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null, MySqlTransaction? transaction = null)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query, parameters);
            if (transaction != null)
                cmd.Transaction = transaction;

            return await cmd.ExecuteScalarAsync();
        }

        /// <summary>
        /// Executes a query that modifies data (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional dictionary of parameters (name-value pairs).</param>
        /// <param name="transaction">Optional transaction to execute the command in.</param>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null, MySqlTransaction? transaction = null)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query, parameters);
            if (transaction != null)
                cmd.Transaction = transaction;

            return await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Creates and configures a MySqlCommand with input parameters.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional dictionary of parameters.</param>
        /// <returns>A configured <see cref="MySqlCommand"/>.</returns>
        private MySqlCommand CreateCommand(string query, Dictionary<string, object>? parameters)
        {
            var cmd = new MySqlCommand
            {
                Connection = _connection,
                CommandText = query,
                CommandType = CommandType.Text
            };

            if (parameters != null)
            {
                foreach (var kv in parameters)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = kv.Key ?? string.Empty;
                    param.Value = kv.Value ?? DBNull.Value;

                    // Basic type mapping for better precision than AddWithValue
                    switch (kv.Value)
                    {
                        case short: param.DbType = DbType.Int16; break;
                        case int: param.DbType = DbType.Int32; break;
                        case long: param.DbType = DbType.Int64; break;
                        case bool: param.DbType = DbType.Boolean; break;
                        case byte[]: param.DbType = DbType.Binary; break;
                        case DateTime: param.DbType = DbType.DateTime; break;
                        case decimal: param.DbType = DbType.Decimal; break;
                        case float: param.DbType = DbType.Single; break;
                        case double: param.DbType = DbType.Double; break;
                        case Guid: param.DbType = DbType.Guid; break;
                        default: param.DbType = DbType.String; break; // Default to string for unknown types
                    }

                    cmd.Parameters.Add(param);
                }
            }

            return cmd;
        }

        /// <summary>
        /// Disposes of the database context and closes the connection if it is open.
        /// </summary>
        /// <remarks>
        /// Connection lifetime is managed by DI, therefore it’s only closed if it’s open.
        /// </remarks>
        public void Dispose()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }
}
