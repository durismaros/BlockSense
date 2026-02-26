using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Contracts.DTOs.Session;
using Dapper;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    public sealed class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly DatabaseContext _databaseContext;

        public RefreshTokenRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    token_hash            AS TokenHash,
                    user_id               AS UserId,
                    ip_address            AS IpAddress,
                    device_identifier     AS DeviceIdentifier,
                    device_os             AS DeviceOs,
                    hardware_fingerprint  AS HardwareFingerprint,
                    network_fingerprint   AS NetworkFingerprint,
                    issued_at             AS IssuedAt,
                    expires_at            AS ExpiresAt,
                    is_revoked            AS IsRevoked
                FROM refresh_tokens
                WHERE token_hash = @TokenHash
                LIMIT 1
                """;

            var parameters = new[]
            {
                new MySqlParameter("@TokenHash", MySqlDbType.VarChar) { Value = tokenHash },
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<RefreshToken>(reader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<RefreshToken?> GetByHardwareFingerprintAsync(string hardwareFingerprint, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    token_hash            AS TokenHash,
                    user_id               AS UserId,
                    ip_address            AS IpAddress,
                    device_identifier     AS DeviceIdentifier,
                    device_os             AS DeviceOs,
                    hardware_fingerprint  AS HardwareFingerprint,
                    network_fingerprint   AS NetworkFingerprint,
                    issued_at             AS IssuedAt,
                    expires_at            AS ExpiresAt,
                    is_revoked            AS IsRevoked
                FROM refresh_tokens
                WHERE hardware_fingerprint = @HardwareFingerprint
                LIMIT 1
                """;

            var parameters = new[]
            {
                new MySqlParameter("@HardwareFingerprint", MySqlDbType.String) { Value = hardwareFingerprint },
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<RefreshToken>(reader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    token_hash            AS TokenHash,
                    user_id               AS UserId,
                    ip_address            AS IpAddress,
                    device_identifier     AS DeviceIdentifier,
                    device_os             AS DeviceOs,
                    hardware_fingerprint  AS HardwareFingerprint,
                    network_fingerprint   AS NetworkFingerprint,
                    issued_at             AS IssuedAt,
                    expires_at            AS ExpiresAt,
                    is_revoked            AS IsRevoked
                FROM refresh_tokens
                WHERE user_id   = @UserId
                    AND is_revoked = 0
                        AND expires_at > UTC_TIMESTAMP(6)
                ORDER BY issued_at DESC
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId },
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<RefreshToken>(reader).AsList().AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            const string sql = """
                REPLACE INTO refresh_tokens (
                    token_hash,
                    user_id,
                    ip_address,
                    device_identifier,
                    device_os,
                    hardware_fingerprint,
                    network_fingerprint,
                    issued_at,
                    expires_at,
                    is_revoked )
                VALUES (
                    @TokenHash,
                    @UserId,
                    @IpAddress,
                    @DeviceIdentifier,
                    @DeviceOs,
                    @HardwareFingerprint,
                    @NetworkFingerprint,
                    @IssuedAt,
                    @ExpiresAt,
                    @IsRevoked );
                """;

            var parameters = new[]
            {
                new MySqlParameter("@TokenHash",           MySqlDbType.VarChar, 255) { Value = refreshToken.TokenHash },
                new MySqlParameter("@UserId",              MySqlDbType.UInt32)       { Value = refreshToken.UserId },
                new MySqlParameter("@IpAddress",           MySqlDbType.VarChar,  45) { Value = refreshToken.IpAddress },
                new MySqlParameter("@DeviceIdentifier",    MySqlDbType.VarChar, 255) { Value = refreshToken.DeviceIdentifier },
                new MySqlParameter("@DeviceOs",            MySqlDbType.VarChar, 255) { Value = refreshToken.DeviceOs },
                new MySqlParameter("@HardwareFingerprint", MySqlDbType.String,   44) { Value = refreshToken.HardwareFingerprint },
                new MySqlParameter("@NetworkFingerprint",  MySqlDbType.String,   17) { Value = refreshToken.NetworkFingerprint },
                new MySqlParameter("@IssuedAt",            MySqlDbType.DateTime)     { Value = refreshToken.IssuedAt },
                new MySqlParameter("@ExpiresAt",           MySqlDbType.DateTime)     { Value = refreshToken.ExpiresAt },
                new MySqlParameter("@IsRevoked",           MySqlDbType.Bit)          { Value = refreshToken.IsRevoked }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RevokeAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE refresh_tokens
                SET is_revoked = 1
                WHERE token_hash = @TokenHash
                """;

            var parameters = new[]
            {
                new MySqlParameter("@TokenHash", MySqlDbType.VarChar) { Value = tokenHash }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RevokeAllByUserIdAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE refresh_tokens
                SET is_revoked = 1
                WHERE user_id   = @UserId
                    AND is_revoked = 0
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId },
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
        {
            const string sql = """
                DELETE FROM refresh_tokens
                WHERE expires_at <= UTC_TIMESTAMP(6)
            """;

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters: null, cancellationToken);
        }
    }
}
