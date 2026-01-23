using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
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
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<InvitationCodeEntity?> GetByIdAsync(uint invitationCodeId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    invitation_code_id  AS InvitationCodeId,
                    invitation_code     AS Code,
                    is_used             AS IsUsed,
                    generated_by        AS GeneratedBy,
                    created_at          AS CreatedAt,
                    expires_at          AS ExpiresAt,
                    is_revoked          AS IsRevoked
                FROM invitation_codes
                WHERE invitation_code_id = @InvitationCodeId
                LIMIT 1;
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationCodeId", MySqlDbType.UInt32)
                {
                    Value = invitationCodeId,
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
                    invitation_code_id  AS InvitationCodeId,
                    invitation_code     AS Code,
                    is_used             AS IsUsed,
                    generated_by        AS GeneratedBy,
                    created_at          AS CreatedAt,
                    expires_at          AS ExpiresAt,
                    is_revoked          AS IsRevoked
                FROM invitation_codes
                WHERE invitation_code = @InvitationCode
                LIMIT 1;
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationCode", MySqlDbType.String)
                {
                    Value = invitationCode,
                }
            };

            await using MySqlDataReader dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCodeEntity>(dbReader).FirstOrDefault();
        }

        public async Task<InvitationCodeEntity?> GetByCodeForUpdateAsync(string invitationCode, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT
                    invitation_code_id  AS InvitationCodeId,
                    invitation_code     AS Code,
                    is_used             AS IsUsed,
                    generated_by        AS GeneratedBy,
                    created_at          AS CreatedAt,
                    expires_at          AS ExpiresAt,
                    is_revoked          AS IsRevoked
                FROM invitation_codes
                WHERE invitation_code = @InvitationCode
                LIMIT 1
                FOR UPDATE;
            """;

            var parameters = new[]
{
                new MySqlParameter("@InvitationCode", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCode,
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
                    invitation_code_id  AS InvitationCodeId,
                    invitation_code     AS InvitationCode,
                    is_used             AS IsUsed,
                    generated_by        AS GeneratedBy,
                    created_at          AS CreatedAt,
                    expires_at          AS ExpiresAt,
                    is_revoked          AS IsRevoked
                FROM invitation_codes
                WHERE generated_by = @UserId
                ORDER BY is_used ASC;
            """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32)
                {
                    Value = userId,
                }
            };

            await using MySqlDataReader dbReader =
                await _databaseContext.ExecuteReaderAsync(sqlQuery, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCodeEntity>(dbReader).ToList();
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
                    Value = invitationCode,
                }
            };

            var result = await _databaseContext.ExecuteScalarAsync<long>(sqlQuery, parameters, cancellationToken);

            return result > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> IsCodeActiveAsync(string invitationCode, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                SELECT COUNT(1)
                FROM invitation_codes
                WHERE invitation_code = @InvitationCode
                    AND is_used = 0
                    AND is_revoked = 0
                    AND (expires_at IS NULL OR expires_at > UTC_TIMESTAMP(6));
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationCode", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCode,
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
                    is_used,
                    generated_by,
                    created_at,
                    expires_at,
                    is_revoked )
                VALUES (
                    @InvitationCode,
                    @IsUsed,
                    @GeneratedBy,
                    @CreatedAt,
                    @ExpiresAt,
                    @IsRevoked );
                SELECT LAST_INSERT_ID();
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationCode", MySqlDbType.VarChar, 32) { Value = invitation.InvitationCode },
                new MySqlParameter("@IsUsed", MySqlDbType.Bit) { Value = invitation.IsUsed },
                new MySqlParameter("@GeneratedBy", MySqlDbType.UInt32) { Value = invitation.GeneratedBy },
                new MySqlParameter("@CreatedAt", MySqlDbType.DateTime) { Value = invitation.CreatedAt },
                new MySqlParameter("@ExpiresAt", MySqlDbType.DateTime) { Value = invitation.ExpiresAt.HasValue ? invitation.ExpiresAt.Value : DBNull.Value },
                new MySqlParameter("@IsRevoked", MySqlDbType.Bit) { Value = invitation.IsRevoked }
            };

            var result = await _databaseContext.ExecuteScalarAsync<ulong>(sqlQuery, parameters, cancellationToken);

            return Convert.ToUInt32(result);
        }

        /// <inheritdoc/>
        public async Task MarkAsUsedAsync(uint invitationCodeId, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                UPDATE invitation_codes
                SET is_used = 1
                WHERE invitation_code_id = @InvitationCodeId
                  AND is_used = 0;
            """;

            var parameters = new[]
            {
                new MySqlParameter("@InvitationCodeId", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCodeId
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
                WHERE invitation_code_id = @InvitationCodeId
                  AND is_revoked = 0;
            """;

            var parameters = new[]
{
                new MySqlParameter("@InvitationCodeId", MySqlDbType.VarChar, 32)
                {
                    Value = invitationCodeId
                }
            };

            await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }
    }
}
