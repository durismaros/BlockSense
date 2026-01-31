using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;
using System.Text.Json;
using System.Threading;
using static Dapper.SqlMapper;

namespace BlockSense.Backend.Repositories.Implementations
{
    /// <summary>
    /// Provides data access methods for <see cref="TwoFactorAuthEntity"/> objects.
    /// </summary>
    public sealed class TwoFactorAuthRepository : ITwoFactorAuthRepository
    {
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of <see cref="TwoFactorAuthRepository"/> with the provided <see cref="DatabaseContext"/>.
        /// </summary>
        /// <param name="databaseContext">The database context used to execute SQL queries.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="databaseContext"/> is <c>null</c>.</exception>
        public TwoFactorAuthRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<TwoFactorAuthEntity?> GetByUserIdAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    user_id                  AS UserId,
                    encrypted_totp_secret    AS EncryptedTotpSecret,
                    backup_codes             AS BackupCodesJson,
                    updated_at               AS UpdatedAt
                FROM two_factor_auth
                WHERE user_id = @UserId
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32)
                {
                    Value = userId
                }
            };

            await using var dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<TwoFactorAuthEntity>(dbReader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<bool> IsEnabledAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                    SELECT COUNT(1)
                    FROM two_factor_auth
                    WHERE user_id = @UserId;
                """;

            var parameters = new[]
{
                new MySqlParameter("@UserId", MySqlDbType.UInt32)
                {
                    Value = userId
                }
            };

            var result = await _databaseContext.ExecuteScalarAsync<long>(sqlQuery, parameters, cancellationToken);

            return result > 0;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(TwoFactorAuthEntity entity, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                INSERT INTO two_factor_auth (
                    user_id,
                    encrypted_totp_secret,
                    backup_codes,
                    updated_at )
                VALUES (
                    @UserId,
                    @EncryptedTotpSecret,
                    @BackupCodesJson,
                    @UpdatedAt );
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = entity.UserId },
                new MySqlParameter("@EncryptedTotpSecret", MySqlDbType.Binary, 48) { Value = entity.EncryptedTotpSecret },
                new MySqlParameter("@BackupCodesJson", MySqlDbType.JSON) { Value = (object)JsonSerializer.Serialize(entity.BackupCodes) ?? DBNull.Value },
                new MySqlParameter("@UpdatedAt", MySqlDbType.DateTime) { Value = entity.UpdatedAt }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdateBackupCodesAsync(uint userId, IReadOnlyList<string> backupCodes, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE two_factor_auth
                SET backup_codes = @BackupCodesJson
                WHERE
                    user_id = @UserId
                    AND backup_codes IS NOT NULL
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32)
                {
                    Value = userId
                },
                new MySqlParameter("BackupCodesJson", MySqlDbType.JSON)
                {
                    Value = JsonSerializer.Serialize(backupCodes)
                }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task InsertBackupCodesAsync(uint userId, IReadOnlyList<string> backupCodes, DateTime updatedAt, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE two_factor_auth
                SET
                    backup_codes = @BackupCodesJson,
                    updated_at = @UpdatedAt
                WHERE user_id = @UserId;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId },
                new MySqlParameter("@BackupCodesJson", MySqlDbType.JSON) { Value = JsonSerializer.Serialize(backupCodes) },
                new MySqlParameter("@UpdatedAt", MySqlDbType.DateTime) { Value = updatedAt }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DisableAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                DELETE FROM two_factor_auth
                WHERE user_id = @UserId;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32)
                {
                    Value = userId
                }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }
    }
}
