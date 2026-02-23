using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Contracts.Enums;
using Dapper;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _databaseContext;

        public UserRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<User?> GetByIdAsync(uint userId, CancellationToken cancellationToken = default)
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
                    AND deleted_at IS NULL
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.UInt32) { Value = userId }
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
                WHERE ( username = @Identifier OR email = @Identifier )
                    AND deleted_at IS NULL
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
        public async Task<string?> GetInviterUsernameByUserAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT u.username
                FROM users u
                JOIN invitation_codes ic ON u.id = ic.generated_by
                WHERE ic.used_by = @UserId
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId }
            };

            return await _databaseContext.ExecuteScalarAsync<string?>(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM users
                WHERE username = @Username
                    AND deleted_at IS NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Username", MySqlDbType.VarChar, 32) { Value = username }
            };

            var count =
                await _databaseContext.ExecuteScalarAsync<long>(sql, parameters, cancellationToken);

            return count > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM users
                WHERE email = @Email
                    AND deleted_at IS NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Email", MySqlDbType.VarChar, 256) { Value = email }
            };

            var count =
                await _databaseContext.ExecuteScalarAsync<long>(sql, parameters, cancellationToken);

            return count > 0;
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
                    updated_at )
                VALUES (
                    @Username,
                    @Email,
                    @Role,
                    @PasswordHash,
                    @PasswordSalt,
                    @CreatedAt,
                    @UpdatedAt );
                SELECT LAST_INSERT_ID();
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Username",     MySqlDbType.VarChar,  32) { Value = user.Username },
                new MySqlParameter("@Email",        MySqlDbType.VarChar, 256) { Value = user.Email },
                new MySqlParameter("@Role",         MySqlDbType.Enum)         { Value = user.Role.ToString().ToLowerInvariant() },
                new MySqlParameter("@PasswordHash", MySqlDbType.Binary,   32) { Value = user.PasswordHash },
                new MySqlParameter("@PasswordSalt", MySqlDbType.Binary,   16) { Value = user.PasswordSalt },
                new MySqlParameter("@CreatedAt",    MySqlDbType.DateTime)     { Value = user.CreatedAt },
                new MySqlParameter("@UpdatedAt",    MySqlDbType.DateTime)     { Value = user.UpdatedAt }
            };

            var insertId =
                await _databaseContext.ExecuteScalarAsync<ulong>(sql, parameters, cancellationToken);

            return Convert.ToUInt32(insertId);
        }

        /// <inheritdoc/>
        public async Task UpdateRoleAsync(
            uint userId,
            UserRole role,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE users
                SET role = @Role
                WHERE id = @Id
                    AND deleted_at IS NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id",   MySqlDbType.UInt32) { Value = userId },
                new MySqlParameter("@Role", MySqlDbType.Enum)   { Value = role.ToString().ToLowerInvariant() }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task SoftDeleteAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE users
                SET deleted_at = UTC_TIMESTAMP(6)
                WHERE id = @Id
                    AND deleted_at IS NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.UInt32) { Value = userId }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RestoreAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE users
                SET deleted_at = NULL
                WHERE id = @Id
                    AND deleted_at IS NOT NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.UInt32) { Value = userId }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }
    }
}
