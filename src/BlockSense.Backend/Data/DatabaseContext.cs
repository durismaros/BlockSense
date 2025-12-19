using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace BlockSense.Backend.Data
{
    /// <summary>
    /// Represents a database context for interacting with MySQL database.
    /// </summary>
    public class DatabaseContext : IAsyncDisposable
    {
        private readonly MySqlConnection _connection;
        private MySqlTransaction? _currentTransaction;

        /// <summary>
        /// Gets the underlying MySQL connection.
        /// </summary>
        public MySqlConnection Connection => _connection;

        /// <summary>
        /// Initializes a new instance of <see cref="DatabaseContext"/> class with the provided MySqlConnection.
        /// </summary>
        /// <param name="connection">The MySQL connection to use. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> is null.</exception>
        public DatabaseContext(MySqlConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MySqlTransaction? GetCurrentTransaction() => _currentTransaction;

        /// <summary>
        /// Begins a new database transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">The transaction isolation level. Defaults to <see cref="IsolationLevel.ReadCommitted"/>.</param>
        /// <returns>A <see cref="MySqlTransaction"/> representing the new transaction.</returns>
        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            await EnsureConnectionOpenAsync();
            _currentTransaction = _connection.BeginTransaction(isolationLevel);
        }

        public async Task CommitAsync()
        {
            if (_currentTransaction == null) throw new InvalidOperationException("No active transaction to commit.");
            await _currentTransaction.CommitAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async Task RollbackAsync()
        {
            if (_currentTransaction == null) return;
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        /// <summary>
        /// Executes a SQL query that returns multiple rows.
        /// </summary>
        /// <param name="query">The SQL query text.</param>
        /// <param name="transaction">Optional transaction to execute the command in.</param>
        /// <param name="parameters">Optional SQL parameters.</param>
        /// <returns>A <see cref="DbDataReader"/> for reading the query results.</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(string query, params MySqlParameter[] parameters)
        {
            await EnsureConnectionOpenAsync();
            var command = CreateCommand(query, _currentTransaction, parameters);
            return await command.ExecuteReaderAsync(CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a SQL query that returns a single scalar value.
        /// </summary>
        /// <param name="query">The SQL query text.</param>
        /// <param name="transaction">Optional transaction to execute the command in.</param>
        /// <param name="parameters">Optional SQL parameters.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public async Task<object?> ExecuteScalarAsync(string query, params MySqlParameter[] parameters)
        {
            await EnsureConnectionOpenAsync();
            await using var command = CreateCommand(query, _currentTransaction, parameters);
            return await command.ExecuteScalarAsync();
        }

        /// <summary>
        /// Executes a SQL command that modifies data (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="query">The SQL command text.</param>
        /// <param name="transaction">Optional transaction to execute the command in.</param>
        /// <param name="parameters">Optional SQL parameters.</param>
        /// <returns>The number of rows affected by the command.</returns>
        public async Task<int> ExecuteNonQueryAsync(string query, params MySqlParameter[] parameters)
        {
            await EnsureConnectionOpenAsync();
            await using var command = CreateCommand(query, _currentTransaction, parameters);
            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Ensures the underlying database connection is open.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task EnsureConnectionOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        /// <summary>
        /// Creates a <see cref="MySqlCommand"/> configured with the specified query, transaction, and parameters.
        /// </summary>
        /// <param name="query">The SQL query or command text.</param>
        /// <param name="transaction">Optional transaction to associate with the command.</param>
        /// <param name="parameters">Optional SQL parameters.</param>
        /// <returns>A configured <see cref="MySqlCommand"/>.</returns>
        private MySqlCommand CreateCommand(string query, MySqlTransaction? transaction, MySqlParameter[]? parameters)
        {
            var command = _connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.Transaction = transaction;

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command;
        }

        /// <summary>
        /// Disposes of the database context and closes the connection if it is open.
        /// </summary>
        /// <remarks>Connection lifetime is managed externally, so this only closes it if still open.</remarks>
        public async ValueTask DisposeAsync()
        {
            if (_connection.State == ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }

            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
            }

            await _connection.DisposeAsync();
        }
    }
}
