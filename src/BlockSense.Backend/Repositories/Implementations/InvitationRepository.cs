using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Contracts.DTOs.Invitation;
using Dapper;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    /// <summary>
    /// Provides data access methods for <see cref="InvitationCodeEntity"/> objects.
    /// </summary>
    public sealed class InvitationRepository : IInvitationRepository
    {
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of <see cref="InvitationRepository"/> with the provided <see cref="DatabaseContext"/>.
        /// </summary>
        /// <param name="databaseContext">The database context used to execute SQL queries.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="databaseContext"/> is <c>null</c>.</exception>
        public InvitationRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<InvitationCodeEntity?> GetByIdAsync(uint invitationCodeId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    invitation_id       AS InvitationId,
                    invitation_code     AS InvitationCode,
                    generated_by        AS GeneratedBy,
                    used_by             AS UsedBy,
                    created_at          AS CreatedAt,
                    expires_at          AS ExpiresAt,
                    is_revoked          AS IsRevoked
                FROM invitation_codes
                WHERE invitation_id = @InvitationId
                LIMIT 1;
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationId", MySqlDbType.UInt32)
                {
                    Value = invitationCodeId
                }
            };

            await using MySqlDataReader dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCodeEntity>(dbReader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<InvitationCodeEntity?> GetByCodeAsync(string invitationCode, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    invitation_id       AS InvitationId,
                    invitation_code     AS InvitationCode,
                    generated_by        AS GeneratedBy,
                    used_by             AS UsedBy,
                    created_at          AS CreatedAt,
                    expires_at          AS ExpiresAt,
                    is_revoked          AS IsRevoked
                FROM invitation_codes
                WHERE invitation_code = @InvitationCode
                LIMIT 1;
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationCode", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCode
                }
            };

            await using MySqlDataReader dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCodeEntity>(dbReader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<InvitationCodeEntity?> GetByCodeForUpdateAsync(string invitationCode, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    invitation_id       AS InvitationId,
                    invitation_code     AS InvitationCode,
                    generated_by        AS GeneratedBy,
                    used_by             AS UsedBy,
                    created_at          AS CreatedAt,
                    expires_at          AS ExpiresAt,
                    is_revoked          AS IsRevoked
                FROM invitation_codes
                WHERE invitation_code = @InvitationCode
                    AND used_by is NULL
                    AND is_revoked = 0
                    AND expires_at > UTC_TIMESTAMP(6)
                LIMIT 1
                FOR UPDATE;
            """;

            var parameters = new[]
{
                new MySqlParameter("@InvitationCode", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCode
                }
            };

            await using MySqlDataReader dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCodeEntity>(dbReader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InvitationCodeEntity>> GetByUserAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    invitation_id        AS InvitationId,
                    invitation_code      AS InvitationCode,
                    generated_by         AS GeneratedBy,
                    used_by              AS UsedBy,
                    created_at           AS CreatedAt,
                    expires_at           AS ExpiresAt,
                    is_revoked           AS IsRevoked
                FROM invitation_codes
                WHERE generated_by = @UserId
                ORDER BY
                    used_by IS NULL DESC,
                    invitation_id ASC;
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

            return SqlMapper.Parse<InvitationCodeEntity>(dbReader).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InvitationDto>> GetDtoByUserAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    ic.invitation_code AS InvitationCode,
                    ic.created_at      AS CreatedAt,
                    ic.expires_at      AS ExpiresAt,
                    u.username         AS InvitedUser,
                CASE
                    WHEN ic.is_revoked = 1 THEN 'Revoked'
                    WHEN ic.used_by IS NOT NULL THEN 'Used'
                    WHEN ic.expires_at < UTC_TIMESTAMP(6) THEN 'Expired'
                    ELSE 'Active'
                END AS Status
                FROM invitation_codes ic
                LEFT JOIN users u
                    ON ic.used_by = u.user_id
                WHERE ic.generated_by = @UserId
                ORDER BY
                    ic.used_by IS NULL DESC,
                    ic.invitation_id ASC;
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

            return SqlMapper.Parse<InvitationDto>(dbReader).ToList();
        }

        /// <inheritdoc/>
        public async Task<string?> GetInviterUsernameByUser(uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    users.username
                FROM invitation_codes
                JOIN users
                    ON invitation_codes.generated_by = users.user_id
                WHERE used_by = @UserId
            """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32)
                {
                    Value = userId
                }
            };

            return await _databaseContext.ExecuteScalarAsync<string?>(sqlQuery, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> CodeExistsAsync(string invitationCode, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT COUNT(1)
                FROM invitation_codes
                WHERE invitation_code = @InvitationCode;
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationCode", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCode
                }
            };

            var result = await _databaseContext.ExecuteScalarAsync<long>(sqlQuery, parameters, cancellationToken);

            return result > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> IsActiveAsync(string invitationCode, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT COUNT(1)
                FROM invitation_codes
                WHERE invitation_code = @InvitationCode
                    AND used_by is NULL
                    AND is_revoked = 0
                    AND expires_at > UTC_TIMESTAMP(6);
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationCode", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCode
                }
            };

            var result = await _databaseContext.ExecuteScalarAsync<long>(sqlQuery, parameters, cancellationToken);

            return result > 0;
        }

        /// <inheritdoc/>
        public async Task<uint> CreateAsync(InvitationCodeEntity invitation, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                INSERT INTO invitation_codes (
                    invitation_code,
                    generated_by,
                    used_by,
                    created_at,
                    expires_at,
                    is_revoked )
                VALUES (
                    @InvitationCode,
                    @GeneratedBy,
                    @UsedBy,
                    @CreatedAt,
                    @ExpiresAt,
                    @IsRevoked );
                SELECT LAST_INSERT_ID();
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationCode", MySqlDbType.VarChar, 32) { Value = invitation.InvitationCode },
                new MySqlParameter("@GeneratedBy", MySqlDbType.UInt32) { Value = invitation.GeneratedBy },
                new MySqlParameter("@UsedBy", MySqlDbType.UInt32) { Value = invitation.UsedBy },
                new MySqlParameter("@CreatedAt", MySqlDbType.DateTime) { Value = invitation.CreatedAt },
                new MySqlParameter("@ExpiresAt", MySqlDbType.DateTime) { Value = invitation.ExpiresAt },
                new MySqlParameter("@IsRevoked", MySqlDbType.Bit) { Value = invitation.IsRevoked }
            };

            var result = await _databaseContext.ExecuteScalarAsync<ulong>(sqlQuery, parameters, cancellationToken);

            return Convert.ToUInt32(result);
        }

        /// <inheritdoc/>
        public async Task MarkAsUsedAsync(uint invitationCodeId, uint userId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE invitation_codes
                SET used_by = @UserId
                WHERE invitation_id = @InvitationId
                    AND used_by is NULL
                    AND is_revoked = 0
                    AND expires_at > UTC_TIMESTAMP(6);
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationId", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCodeId
                },
                new MySqlParameter("@UserId", MySqlDbType.UInt32)
                {
                    Value = userId
                }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RevokeAsync(uint invitationCodeId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE invitation_codes
                SET is_revoked = 1
                WHERE invitation_id = @InvitationId
                  AND is_revoked = 0;
            """;

            var parameters = new[]
{
                new MySqlParameter("@InvitationId", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCodeId
                }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }
    }
}
