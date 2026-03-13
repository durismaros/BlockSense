using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Contracts.Enums;
using Dapper;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    /// <summary>
    /// MySQL implementation of <see cref="IUserRepository"/>.
    /// </summary>
    public sealed class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of <see cref="UserRepository"/>.
        /// </summary>
        /// <param name="databaseContext">The database context used to execute queries.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="databaseContext"/> is null.</exception>
        public UserRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<User?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id              AS Id,
                    username        AS Username,
                    email           AS Email,
                    role            AS Role,
                    password_hash   AS PasswordHash,
                    password_salt   AS PasswordSalt,
                    created_at      AS CreatedAt,
                    updated_at      AS UpdatedAt,
                    deleted_at      AS DeletedAt
                FROM users
                WHERE id = @Id
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.UInt32) { Value = id }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<User>(reader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<User?> GetByUsernameOrEmailAsync(string identifier, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id              AS Id,
                    username        AS Username,
                    email           AS Email,
                    role            AS Role,
                    password_hash   AS PasswordHash,
                    password_salt   AS PasswordSalt,
                    created_at      AS CreatedAt,
                    updated_at      AS UpdatedAt,
                    deleted_at      AS DeletedAt
                FROM users
                WHERE username = @Identifier
                   OR email    = @Identifier
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Identifier", MySqlDbType.VarChar, 256) { Value = identifier }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<User>(reader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id              AS Id,
                    username        AS Username,
                    email           AS Email,
                    role            AS Role,
                    password_hash   AS PasswordHash,
                    password_salt   AS PasswordSalt,
                    created_at      AS CreatedAt,
                    updated_at      AS UpdatedAt,
                    deleted_at      AS DeletedAt
                FROM users
                WHERE role = @Role
                ORDER BY created_at ASC;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Role", MySqlDbType.Enum) { Value = role.ToString().ToLowerInvariant() }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<User>(reader).AsList().AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task<string?> GetInviterUsernameAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT u.username AS Username
                FROM invitation_codes ic
                INNER JOIN users u ON u.id = ic.issued_to_id
                WHERE ic.redeemed_by_id = @UserId
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId }
            };

            return await _databaseContext.ExecuteScalarAsync<string?>(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<uint> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            const string sql = """
                INSERT INTO users (
                    username,
                    email,
                    role,
                    password_hash,
                    password_salt,
                    created_at,
                    updated_at,
                    deleted_at )
                VALUES (
                    @Username,
                    @Email,
                    @Role,
                    @PasswordHash,
                    @PasswordSalt,
                    @CreatedAt,
                    @UpdatedAt,
                    @DeletedAt );
                SELECT LAST_INSERT_ID();
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Username",     MySqlDbType.VarChar)  { Value = user.Username },
                new MySqlParameter("@Email",        MySqlDbType.VarChar)  { Value = user.Email },
                new MySqlParameter("@Role",         MySqlDbType.Enum)     { Value = user.Role.ToString().ToLowerInvariant() },
                new MySqlParameter("@PasswordHash", MySqlDbType.Binary)   { Value = user.PasswordHash },
                new MySqlParameter("@PasswordSalt", MySqlDbType.Binary)   { Value = user.PasswordSalt },
                new MySqlParameter("@CreatedAt",    MySqlDbType.DateTime) { Value = user.CreatedAt },
                new MySqlParameter("@UpdatedAt",    MySqlDbType.DateTime) { Value = user.UpdatedAt },
                new MySqlParameter("@DeletedAt",    MySqlDbType.DateTime) { Value = (object?)user.DeletedAt ?? DBNull.Value }
            };

            var insertId =
                await _databaseContext.ExecuteScalarAsync<ulong>(sql, parameters, cancellationToken);

            return Convert.ToUInt32(insertId);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE users
                SET
                    username      = @Username,
                    email         = @Email,
                    role          = @Role,
                    password_hash = @PasswordHash,
                    password_salt = @PasswordSalt,
                    updated_at    = @UpdatedAt,
                    deleted_at    = @DeletedAt
                WHERE id = @Id;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id",           MySqlDbType.UInt32)   { Value = user.Id },
                new MySqlParameter("@Username",     MySqlDbType.VarChar)  { Value = user.Username },
                new MySqlParameter("@Email",        MySqlDbType.VarChar)  { Value = user.Email },
                new MySqlParameter("@Role",         MySqlDbType.Enum)     { Value = user.Role.ToString().ToLowerInvariant() },
                new MySqlParameter("@PasswordHash", MySqlDbType.Binary)   { Value = user.PasswordHash },
                new MySqlParameter("@PasswordSalt", MySqlDbType.Binary)   { Value = user.PasswordSalt },
                new MySqlParameter("@UpdatedAt",    MySqlDbType.DateTime) { Value = user.UpdatedAt },
                new MySqlParameter("@DeletedAt",    MySqlDbType.DateTime) { Value = (object?)user.DeletedAt ?? DBNull.Value }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task SoftDeleteAsync(uint id, DateTime deletedAt, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE users
                SET deleted_at = @DeletedAt
                WHERE id = @Id
                  AND deleted_at IS NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id",        MySqlDbType.UInt32)   { Value = id },
                new MySqlParameter("@DeletedAt", MySqlDbType.DateTime) { Value = deletedAt }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RestoreAsync(uint id, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE users
                SET deleted_at = NULL
                WHERE id = @Id
                  AND deleted_at IS NOT NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.UInt32) { Value = id }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }
    }
}