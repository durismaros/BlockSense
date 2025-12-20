using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Contracts.Enums.User;
using Dapper;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    /// <summary>
    /// Provides data access methods for <see cref="UserEntity"/> objects.
    /// </summary>
    public sealed class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of <see cref="UserRepository"/> with the provided <see cref="DatabaseContext"/>.
        /// </summary>
        /// <param name="databaseContext">The database context used for executing queries.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="databaseContext"/> is null.</exception>
        public UserRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The <see cref="UserEntity"/> if found; otherwise, <c>null</c>.</returns>
        public async Task<UserEntity?> GetByIdAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT                
                    user_id               AS UserId,
                    username              AS Username,
                    email                 AS Email,
                    user_type             AS UserType,
                    password_hash         AS PasswordHash,
                    password_salt         AS PasswordSalt,
                    invitation_code_id    AS InvitationCodeId,
                    created_at            AS CreatedAt,
                    updated_at            AS UpdatedAt,
                    deleted_at            AS DeletedAt
                FROM users
                WHERE user_id = @UserId
                  AND deleted_at IS NULL
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32)
                {
                    Value = userId
                }
            };

            await using MySqlDataReader dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<UserEntity>(dbReader).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a user by username or email.
        /// </summary>
        /// <param name="identifier">The username or email of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The <see cref="UserEntity"/> if found; otherwise, <c>null</c>.</returns>
        public async Task<UserEntity?> GetByUsernameOrEmailAsync(string identifier, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT                
                    user_id             AS UserId,
                    username            AS Username,
                    email               AS Email,
                    user_type           AS UserType,
                    password_hash       AS PasswordHash,
                    password_salt       AS PasswordSalt,
                    invitation_code_id  AS InvitationCodeId,
                    created_at          AS CreatedAt,
                    updated_at          AS UpdatedAt,
                    deleted_at          AS DeletedAt
                FROM users
                WHERE ( username = @Identifier OR email = @Identifier )
                    AND deleted_at IS NULL
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Identifier", MySqlDbType.VarChar, 50)
                {
                    Value = identifier
                }
            };

            await using MySqlDataReader dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<UserEntity>(dbReader).FirstOrDefault();
        }

        /// <summary>
        /// Checks if a username already exists in the database.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns><c>true</c> if the username exists; otherwise, <c>false</c>.</returns>
        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT COUNT(1)
                FROM users
                WHERE username = @Username
                    AND deleted_at IS NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Username", MySqlDbType.VarChar, 50)
                {
                    Value = username
                }
            };

            var result = await _databaseContext.ExecuteScalarAsync<long>(sqlQuery, parameters, cancellationToken);

            return result > 0;
        }

        /// <summary>
        /// Checks if an email already exists in the database.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns><c>true</c> if the email exists; otherwise, <c>false</c>.</returns>
        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT COUNT(1)
                FROM users
                WHERE email = @Email
                    AND deleted_at IS NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Email", MySqlDbType.VarChar, 100)
                {
                    Value = email
                }
            };

            var result = await _databaseContext.ExecuteScalarAsync<long>(sqlQuery, parameters, cancellationToken);

            return result > 0;
        }

        /// <summary>
        /// Inserts a new user into the database.
        /// </summary>
        /// <param name="user">The <see cref="UserEntity"/> to insert.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The newly created user's <c>UserId</c>.</returns>
        public async Task<uint> CreateAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                INSERT INTO users (
                    username,
                    email,
                    user_type,
                    password_hash,
                    password_salt,
                    invitation_code_id,
                    created_at,
                    updated_at )
                VALUES (
                    @Username,
                    @Email,
                    @UserType,
                    @PasswordHash,
                    @PasswordSalt,
                    @InvitationCodeId,
                    @CreatedAt,
                    @UpdatedAt );
                SELECT LAST_INSERT_ID();
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Username", MySqlDbType.VarChar, 50) { Value = user.Username },
                new MySqlParameter("@Email", MySqlDbType.VarChar, 100) { Value = user.Email },
                new MySqlParameter("@UserType", MySqlDbType.Enum) { Value = user.UserType },
                new MySqlParameter("@PasswordHash", MySqlDbType.Binary, 32) { Value = user.PasswordHash },
                new MySqlParameter("@PasswordSalt", MySqlDbType.Binary, 16) { Value = user.PasswordSalt },
                new MySqlParameter("@InvitationCodeId", MySqlDbType.UInt32) { Value = user.InvitationCodeId },
                new MySqlParameter("@CreatedAt", MySqlDbType.DateTime) { Value = user.CreatedAt },
                new MySqlParameter("@UpdatedAt", MySqlDbType.DateTime) { Value = user.UpdatedAt },
            };

            var result = await _databaseContext.ExecuteScalarAsync<ulong>(sqlQuery, parameters, cancellationToken);

            return Convert.ToUInt32(result);
        }

        /// <summary>
        /// Updates the type of a user (e.g., Standard, Administrator, Banned).
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="newType">The new <see cref="UserType"/> to assign.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateUserTypeAsync(uint userId, UserType newType, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE users
                SET user_type = @UserType
                WHERE user_id = @UserId
                    AND deleted_at IS NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserType", MySqlDbType.Enum) { Value = newType },
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }

        /// <summary>
        /// Soft-deletes a user by setting the <c>deleted_at</c> timestamp.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to delete.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SoftDeleteAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE users
                SET deleted_at = @DeletedAt
                WHERE user_id = @UserId
                  AND deleted_at IS NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@DeletedAt", MySqlDbType.DateTime) { Value = DateTime.UtcNow},
                new MySqlParameter("@UserId", userId) { Value =  userId },
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }

        /// <summary>
        /// Restores a previously soft-deleted user by clearing the <c>deleted_at</c> timestamp.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to restore.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RestoreAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE users
                SET deleted_at = NULL
                WHERE user_id = @UserId
                  AND deleted_at IS NOT NULL;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", userId)
                {
                    Value = userId
                }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }
    }
}
