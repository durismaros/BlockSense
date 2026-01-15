using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    /// <summary>
    /// Provides data access methods for <see cref="RefreshTokenEntity"/> objects.
    /// </summary>
    public sealed class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of <see cref="RefreshTokenRepository"/> with the provided <see cref="DatabaseContext"/>.
        /// </summary>
        /// <param name="databaseContext">The database context used to execute SQL queries.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="databaseContext"/> is <c>null</c>.</exception>
        public RefreshTokenRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<RefreshTokenEntity?> GetByIdAsync(Guid tokenId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    token_id                AS TokenId
                    user_id                 AS UserId
                    token_hash              AS TokenHash
                    ip_address              AS IpAddress
                    device_identifier       AS DeviceIdentifier
                    hardware_fingerprint    AS HardwareFingerprint
                    network_fingerprint     AS NetworkFingerprint
                    device_os               AS DeviceOs
                    issued_at               AS IssuedAt
                    expires_at              AS ExpiresAt
                    is_revoked              AS IsRevoked
                FROM refresh_tokens
                WHERE token_id = @TokenId
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@TokenId", MySqlDbType.String)
                {
                    Value = tokenId.ToString()
                }
            };

            await using MySqlDataReader dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<RefreshTokenEntity>(dbReader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<RefreshTokenEntity?> GetByTokenAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    token_id                AS TokenId
                    user_id                 AS UserId
                    token_hash              AS TokenHash
                    ip_address              AS IpAddress
                    device_identifier       AS DeviceIdentifier
                    hardware_fingerprint    AS HardwareFingerprint
                    network_fingerprint     AS NetworkFingerprint
                    device_os               AS DeviceOs
                    issued_at               AS IssuedAt
                    expires_at              AS ExpiresAt
                    is_revoked              AS IsRevoked
                FROM refresh_tokens
                WHERE token_hash = @TokenHash
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@TokenHash", MySqlDbType.VarChar, 255)
                {
                    Value = tokenHash
                }
            };

            await using MySqlDataReader dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<RefreshTokenEntity>(dbReader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<RefreshTokenEntity>> GetByUserAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    token_id                AS TokenId,
                    user_id                 AS UserId,
                    token_hash              AS TokenHash,
                    ip_address              AS IpAddress,
                    device_identifier       AS DeviceIdentifier,
                    hardware_fingerprint    AS HardwareFingerprint,
                    network_fingerprint     AS NetworkFingerprint,
                    device_os               AS DeviceOs,
                    issued_at               AS IssuedAt,
                    expires_at              AS ExpiresAt,
                    is_revoked              AS IsRevoked
                FROM refresh_tokens
                WHERE user_id = @UserId;
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

            return SqlMapper.Parse<RefreshTokenEntity>(dbReader).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<RefreshTokenEntity>> GetActiveByUserAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    token_id                AS TokenId,
                    user_id                 AS UserId,
                    token_hash              AS TokenHash,
                    ip_address              AS IpAddress,
                    device_identifier       AS DeviceIdentifier,
                    hardware_fingerprint    AS HardwareFingerprint,
                    network_fingerprint     AS NetworkFingerprint,
                    device_os               AS DeviceOs,
                    issued_at               AS IssuedAt,
                    expires_at              AS ExpiresAt,
                    is_revoked              AS IsRevoked
                FROM refresh_tokens
                WHERE user_id = @UserId
                  AND is_revoked = 0
                  AND expires_at > UTC_TIMESTAMP();
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

            return SqlMapper.Parse<RefreshTokenEntity>(dbReader).ToList();
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(RefreshTokenEntity refreshToken, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                INSERT INTO refresh_tokens (
                    token_id,
                    user_id,
                    token_hash,
                    ip_address,
                    device_identifier,
                    hardware_fingerprint,
                    network_fingerprint,
                    device_os,
                    issued_at,
                    expires_at,
                    is_revoked )
                VALUES (
                    @TokenId,
                    @UserId,
                    @TokenHash,
                    @IpAddress,
                    @DeviceIdentifier,
                    @HardwareFingerprint,
                    @NetworkFingerprint,
                    @DeviceOs,
                    @IssuedAt,
                    @ExpiresAt,
                    @IsRevoked)
                ON DUPLICATE KEY UPDATE
                    token_id = VALUES(token_id),
                    token_hash = VALUES(token_hash),
                    user_id = VALUES(user_id),
                    ip_address = VALUES(ip_address),
                    device_identifier = VALUES(device_identifier),
                    device_os = VALUES(device_os),
                    issued_at = VALUES(issued_at),
                    expires_at = VALUES(expires_at),
                    is_revoked = VALUES(is_revoked);
                """;

            var parameters = new[]
            {
                new MySqlParameter("@TokenId", MySqlDbType.String) { Value = refreshToken.TokenId.ToString() },
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = refreshToken.UserId },
                new MySqlParameter("@TokenHash", MySqlDbType.VarChar, 255) { Value = refreshToken.TokenHash },
                new MySqlParameter("@IpAddress", MySqlDbType.VarChar, 45) { Value = refreshToken.IpAddress },
                new MySqlParameter("@DeviceIdentifier", MySqlDbType.VarChar, 255) { Value = refreshToken.DeviceIdentifier },
                new MySqlParameter("@HardwareFingerprint", MySqlDbType.String) { Value = refreshToken.HardwareFingerprint },
                new MySqlParameter("@NetworkFingerprint", MySqlDbType.String) { Value = refreshToken.NetworkFingerprint },
                new MySqlParameter("@DeviceOs", MySqlDbType.VarChar, 150) { Value = refreshToken.DeviceOs },
                new MySqlParameter("@IssuedAt", MySqlDbType.DateTime) { Value = refreshToken.IssuedAt },
                new MySqlParameter("@ExpiresAt", MySqlDbType.DateTime) { Value = refreshToken.ExpiresAt },
                new MySqlParameter("@IsRevoked", MySqlDbType.Bit) { Value = refreshToken.IsRevoked }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);

            return refreshToken.TokenId;
        }

        /// <inheritdoc/>
        public async Task RevokeAsync(Guid tokenId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE refresh_tokens
                SET is_revoked = 1
                WHERE token_id = @TokenId
                    AND is_revoked = 0;
                """;

            var parameters = new[]
{
                new MySqlParameter("@TokenId", MySqlDbType.String)
                {
                    Value = tokenId.ToString()
                }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RevokeAllForUserAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE refresh_tokens
                SET is_revoked = 1
                WHERE user_id = @UserId
                    AND is_revoked = 0;
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
