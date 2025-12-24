using MySql.Data.MySqlClient;
using System.Data;

namespace BlockSense.Backend.Data
{
    /// <summary>
    /// Represents a context for interacting with a MySQL database.
    /// </summary>
    public sealed class DatabaseContext : IAsyncDisposable
    {
        private readonly MySqlConnection _connection;
        private MySqlTransaction? _currentTransaction;

        /// <summary>
        /// Gets the underlying MySQL connection used by this context.
        /// </summary>
        public MySqlConnection Connection => _connection;

        /// <summary>
        /// Initializes a new instance of <see cref="DatabaseContext"/> using the provided connection.
        /// </summary>
        /// <param name="connection">A non-null <see cref="MySqlConnection"/>. Connection lifetime is managed externally.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> is null.</exception>
        public DatabaseContext(MySqlConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// Gets the currently active transaction, if any.
        /// </summary>
        /// <returns>The active <see cref="MySqlTransaction"/> or null if none exists.</returns>
        public MySqlTransaction? GetCurrentTransaction() => _currentTransaction;

        /// <summary>
        /// Begins a new transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">The transaction isolation level. Defaults to <see cref="IsolationLevel.ReadCommitted"/>.</param>
        /// <param name="cancellationToken">Optional token to cancel opening the connection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a transaction is already active.</exception>
        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("A transaction is already active.");

            await EnsureConnectionOpenAsync(cancellationToken);
            _currentTransaction = _connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Commits the currently active transaction.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the commit.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no transaction is active.</exception>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
                throw new InvalidOperationException("No active transaction to commit.");

            await _currentTransaction.CommitAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        /// <summary>
        /// Rolls back the currently active transaction.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the rollback.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        /// <summary>
        /// Executes a SQL query that returns a <see cref="DbDataReader"/> for reading multiple rows.
        /// </summary>
        /// <param name="query">The SQL query to execute. Must not be null or empty.</param>
        /// <param name="parameters">Optional SQL parameters for the query.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="DbDataReader"/> for reading query results.</returns>
        public async Task<MySqlDataReader> ExecuteReaderAsync(string query, IEnumerable<MySqlParameter>? parameters = null, CancellationToken cancellationToken = default)
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            var command = CreateCommand(query, parameters);
            return await command.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
        }

        /// <summary>
        /// Executes a SQL query and returns the first column of the first row as a typed value.
        /// </summary>
        /// <typeparam name="T">The type to cast the scalar value to.</typeparam>
        /// <param name="query">The SQL query to execute. Must not be null or empty.</param>
        /// <param name="parameters">Optional SQL parameters for the query.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The scalar result of the query cast to type <typeparamref name="T"/>, or default if no value is returned.</returns>
        public async Task<T?> ExecuteScalarAsync<T>(string query, IEnumerable<MySqlParameter>? parameters = null, CancellationToken cancellationToken = default)
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            await using var command = CreateCommand(query, parameters);
            var result =  await command.ExecuteScalarAsync(cancellationToken);

            return (result is null || result == DBNull.Value) ? default : (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// Executes a SQL command that does not return a result set (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="query">The SQL query to execute. Must not be null or empty.</param>
        /// <param name="parameters">Optional SQL parameters for the query.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> ExecuteNonQueryAsync(string query, IEnumerable<MySqlParameter>? parameters = null, CancellationToken cancellationToken = default)
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            await using var command = CreateCommand(query, parameters);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Ensures the underlying connection is open before executing commands.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel opening the connection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken)
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Creates a <see cref="MySqlCommand"/> with the provided SQL, parameters, and current transaction.
        /// </summary>
        /// <param name="query">The SQL query to execute. Must not be null or empty.</param>
        /// <param name="parameters">Optional parameters for the query.</param>
        /// <returns>A configured <see cref="MySqlCommand"/> ready for execution.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null or empty or a parameter is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if duplicate parameter names are detected.</exception>
        private MySqlCommand CreateCommand(string query, IEnumerable<MySqlParameter>? parameters)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException(nameof(query), "SQL query must not be empty.");
            }

            var command = _connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 30;
            command.Transaction = _currentTransaction;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p is null)
                        throw new ArgumentNullException(nameof(parameters));

                    if (command.Parameters.Contains(p.ParameterName))
                        throw new InvalidOperationException( $"Duplicate SQL parameter detected: {p.ParameterName}");

                    command.Parameters.Add(p);
                }
            }

            return command;
        }

        /// <summary>
        /// Asynchronously disposes the database context.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }

            if (_connection.State == ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }

            await _connection.DisposeAsync();
        }
    }
}
