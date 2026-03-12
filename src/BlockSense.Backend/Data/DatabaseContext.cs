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
        /// Initializes a new instance of <see cref="DatabaseContext"/> using the provided connection.
        /// </summary>
        /// <param name="connection">A non-null <see cref="MySqlConnection"/>. Connection lifetime is managed externally.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> is null.</exception>
        public DatabaseContext(MySqlConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// Begins a new transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">The transaction isolation level. Defaults to <see cref="IsolationLevel.ReadCommitted"/>.</param>
        /// <param name="cancellationToken">Optional token to cancel opening the connection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a transaction is already active.</exception>
        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is not null)
            {
                throw new InvalidOperationException("A transaction is already active.");
            }

            await EnsureConnectionOpenAsync(cancellationToken);
            _currentTransaction = await _connection.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        /// <summary>
        /// Commits the currently active transaction.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the commit.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no transaction is active.</exception>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            await _currentTransaction.CommitAsync(cancellationToken);
            await DisposeTransactionAsync();
        }

        /// <summary>
        /// Rolls back the currently active transaction.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the rollback.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no transaction is active.</exception>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }

            await _currentTransaction.RollbackAsync(cancellationToken);
            await DisposeTransactionAsync();
        }

        /// <summary>
        /// Executes a SQL query and returns a <see cref="MySqlDataReader"/> for reading multiple rows.
        /// </summary>
        /// <param name="query">The SQL query to execute. Must not be null or empty.</param>
        /// <param name="parameters">Optional SQL parameters for the query.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="MySqlDataReader"/> for reading the query results.</returns>
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
        /// <returns>The scalar result cast to <typeparamref name="T"/>, or <c>default</c> if no value is returned.</returns>
        public async Task<T?> ExecuteScalarAsync<T>(string query, IEnumerable<MySqlParameter>? parameters = null, CancellationToken cancellationToken = default)
        {
            await EnsureConnectionOpenAsync(cancellationToken);

            await using var command = CreateCommand(query, parameters);
            var result = await command.ExecuteScalarAsync(cancellationToken);

            return IsNullOrDbNull(result) ? default : (T)Convert.ChangeType(result!, typeof(T));
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
        /// Asynchronously disposes the database context, rolling back any active transaction
        /// and closing the underlying connection.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.RollbackAsync();
                await DisposeTransactionAsync();
            }

            if (_connection.State == ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }

            await _connection.DisposeAsync();
        }

        /// <summary>
        /// Ensures the underlying database connection is open before executing commands.
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
        /// Disposes the current transaction and clears the reference.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task DisposeTransactionAsync()
        {
            await _currentTransaction!.DisposeAsync();
            _currentTransaction = null;
        }

        /// <summary>
        /// Creates a <see cref="MySqlCommand"/> configured with the provided SQL, parameters, and active transaction.
        /// </summary>
        /// <param name="query">The SQL query to execute. Must not be null or empty.</param>
        /// <param name="parameters">Optional parameters to bind to the command.</param>
        /// <returns>A configured <see cref="MySqlCommand"/> ready for execution.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="query"/> is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if any parameter in <paramref name="parameters"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if duplicate parameter names are detected.</exception>
        private MySqlCommand CreateCommand(string query, IEnumerable<MySqlParameter>? parameters)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("SQL query must not be null or empty.", nameof(query));
            }

            var command = _connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 30;
            command.Transaction = _currentTransaction;

            if (parameters is not null)
            {
                AddParameters(command, parameters);
            }

            return command;
        }

        /// <summary>
        /// Adds the provided parameters to the given command, validating each one for nulls and duplicates.
        /// </summary>
        /// <param name="command">The command to which parameters will be added.</param>
        /// <param name="parameters">The parameters to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if any individual parameter is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a duplicate parameter name is detected.</exception>
        private static void AddParameters(MySqlCommand command, IEnumerable<MySqlParameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter is null)
                {
                    throw new ArgumentNullException(nameof(parameters), "SQL parameter must not be null.");
                }

                if (command.Parameters.Contains(parameter.ParameterName))
                {
                    throw new InvalidOperationException($"Duplicate SQL parameter detected: {parameter.ParameterName}");
                }

                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Determines whether a scalar query result represents a null or database null value.
        /// </summary>
        /// <param name="value">The value returned from a scalar query.</param>
        /// <returns><c>true</c> if the value is <c>null</c> or <see cref="DBNull.Value"/>; otherwise <c>false</c>.</returns>
        private static bool IsNullOrDbNull(object? value) => value is null || value == DBNull.Value;
    }
}