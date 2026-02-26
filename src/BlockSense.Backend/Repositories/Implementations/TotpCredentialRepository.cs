using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    public sealed class TotpCredentialRepository : ITotpCredentialRepository
    {
        private readonly DatabaseContext _databaseContext;

        public TotpCredentialRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<TotpCredential?> GetByUserIdAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    user_id          AS UserId,
                    encrypted_secret AS EncryptedSecret,
                    backup_codes     AS BackupCodes,
                    created_at       AS CreatedAt,
                    updated_at       AS UpdatedAt
                FROM totp_credentials
                WHERE user_id = @UserId
                LIMIT 1
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId },
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<TotpCredential>(reader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT EXISTS (
                    SELECT 1
                    FROM totp_credentials
                    WHERE user_id = @UserId )
            """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId },
            };

            var result =
                await _databaseContext.ExecuteScalarAsync<ulong>(sql, parameters, cancellationToken);

            return result == 1UL;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(TotpCredential totpCredential, CancellationToken cancellationToken = default)
        {
            const string sql = """
                INSERT INTO totp_credentials (
                    user_id,
                    encrypted_secret,
                    backup_codes,
                    created_at,
                    updated_at )
                VALUES (
                    @UserId,
                    @EncryptedSecret,
                    @BackupCodes,
                    @CreatedAt,
                    @UpdatedAt );
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId",          MySqlDbType.UInt32)        { Value = totpCredential.UserId },
                new MySqlParameter("@EncryptedSecret", MySqlDbType.VarBinary, 48) { Value = totpCredential.EncryptedSecret },
                new MySqlParameter("@BackupCodes",     MySqlDbType.JSON)          { Value = (object?)totpCredential.BackupCodes ?? DBNull.Value },
                new MySqlParameter("@CreatedAt",       MySqlDbType.DateTime)      { Value = totpCredential.CreatedAt },
                new MySqlParameter("@UpdatedAt",       MySqlDbType.DateTime)      { Value = totpCredential.UpdatedAt }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(TotpCredential totpCredential, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE totp_credentials
                SET
                    encrypted_secret = @EncryptedSecret,
                    backup_codes     = @BackupCodes,
                    updated_at       = @UpdatedAt
                WHERE user_id = @UserId
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId",          MySqlDbType.UInt32)          { Value = totpCredential.UserId },
                new MySqlParameter("@EncryptedSecret", MySqlDbType.VarBinary, 48)   { Value = totpCredential.EncryptedSecret },
                new MySqlParameter("@BackupCodes",     MySqlDbType.JSON)            { Value = (object?)totpCredential.BackupCodes ?? DBNull.Value },
                new MySqlParameter("@UpdatedAt",       MySqlDbType.DateTime)        { Value = totpCredential.UpdatedAt },
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteByUserIdAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                DELETE FROM totp_credentials
                WHERE user_id = @UserId
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId },
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }
    }
}
