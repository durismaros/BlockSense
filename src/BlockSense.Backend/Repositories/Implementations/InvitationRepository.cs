using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Contracts.DTOs.Invitation;
using Dapper;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    public sealed class InvitationRepository : IInvitationRepository
    {
        private readonly DatabaseContext _databaseContext;

        public InvitationRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<InvitationCode?> GetByIdAsync(uint invitationId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id              AS Id,
                    code            AS Code,
                    generated_by    AS GeneratedBy,
                    used_by         AS UsedBy,
                    created_at      AS CreatedAt,
                    expires_at      AS ExpiresAt,
                    is_revoked      AS IsRevoked
                FROM invitation_codes
                WHERE id = @Id
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.UInt32) { Value = invitationId }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCode>(reader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<InvitationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id              AS Id,
                    code            AS Code,
                    generated_by    AS GeneratedBy,
                    used_by         AS UsedBy,
                    created_at      AS CreatedAt,
                    expires_at      AS ExpiresAt,
                    is_revoked      AS IsRevoked
                FROM invitation_codes
                WHERE code = @Code
                LIMIT 1;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Code", MySqlDbType.VarChar, 32) { Value = code }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCode>(reader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<InvitationCode?> GetByCodeForUpdateAsync(string code, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id              AS Id,
                    code            AS Code,
                    generated_by    AS GeneratedBy,
                    used_by         AS UsedBy,
                    created_at      AS CreatedAt,
                    expires_at      AS ExpiresAt,
                    is_revoked      AS IsRevoked
                FROM invitation_codes
                WHERE code = @Code
                    AND used_by IS NULL
                    AND is_revoked = 0
                    AND UTC_TIMESTAMP(6) < expires_at
                LIMIT 1
                FOR UPDATE;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Code", MySqlDbType.VarChar, 32) { Value = code }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCode>(reader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InvitationCode>> GetByUserAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id              AS Id,
                    code            AS Code,
                    generated_by    AS GeneratedBy,
                    used_by         AS UsedBy,
                    created_at      AS CreatedAt,
                    expires_at      AS ExpiresAt,
                    is_revoked      AS IsRevoked
                FROM invitation_codes
                WHERE generated_by = @UserId
                ORDER BY
                    used_by IS NULL DESC,
                    id              ASC;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCode>(reader).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InvitationCode>> GetWithInviteeByUserAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    ic.code         AS Code,
                    ic.created_at   AS CreatedAt,
                    ic.expires_at   AS ExpiresAt,
                    u.username      AS InvitedUser
                FROM invitation_codes ic
                LEFT JOIN users u ON ic.used_by = u.id
                WHERE ic.generated_by = @UserId
                ORDER BY
                    ic.used_by IS NULL DESC,
                    ic.id              ASC;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCode>(reader).ToList();
        }

        /// <inheritdoc/>
        public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM invitation_codes
                WHERE code = @Code;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Code", MySqlDbType.VarChar, 32) { Value = code }
            };

            var count =
                await _databaseContext.ExecuteScalarAsync<long>(sql, parameters, cancellationToken);

            return count > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> IsActiveAsync(string code, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM invitation_codes
                WHERE code = @Code
                    AND used_by IS NULL
                    AND is_revoked = 0
                    AND expires_at > UTC_TIMESTAMP(6);
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Code", MySqlDbType.VarChar, 32) { Value = code }
            };

            var count =
                await _databaseContext.ExecuteScalarAsync<long>(sql, parameters, cancellationToken);

            return count > 0;
        }

        /// <inheritdoc/>
        public async Task<uint> CreateAsync(InvitationCode invitation, CancellationToken cancellationToken = default)
        {
            const string sql = """
                INSERT INTO invitation_codes (
                    code,
                    generated_by,
                    used_by,
                    created_at,
                    expires_at,
                    is_revoked )
                VALUES (
                    @Code,
                    @GeneratedBy,
                    @UsedBy,
                    @CreatedAt,
                    @ExpiresAt,
                    @IsRevoked );
                SELECT LAST_INSERT_ID();
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Code",          MySqlDbType.VarChar,   32) { Value = invitation.Code },
                new MySqlParameter("@GeneratedBy", MySqlDbType.UInt32)          { Value = invitation.GeneratedBy },
                new MySqlParameter("@UsedBy",      MySqlDbType.UInt32)          { Value = (object?)invitation.UsedBy ?? DBNull.Value },
                new MySqlParameter("@CreatedAt",     MySqlDbType.DateTime)      { Value = invitation.CreatedAt },
                new MySqlParameter("@ExpiresAt",     MySqlDbType.DateTime)      { Value = invitation.ExpiresAt },
                new MySqlParameter("@IsRevoked",     MySqlDbType.Bit)           { Value = invitation.IsRevoked }
            };

            var insertId =
                await _databaseContext.ExecuteScalarAsync<ulong>(sql, parameters, cancellationToken);

            return Convert.ToUInt32(insertId);
        }

        /// <inheritdoc/>
        public async Task MarkAsUsedAsync(
            uint invitationId,
            uint usedByUserId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE invitation_codes
                SET used_by = @UsedBy
                WHERE id = @Id
                    AND used_by IS NULL
                    AND is_revoked = 0
                    AND expires_at > UTC_TIMESTAMP(6);
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id",     MySqlDbType.UInt32) { Value = invitationId },
                new MySqlParameter("@UsedBy", MySqlDbType.UInt32) { Value = usedByUserId }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RevokeAsync(uint invitationId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE invitation_codes
                SET is_revoked = 1
                WHERE id = @Id
                    AND is_revoked = 0;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.UInt32) { Value = invitationId }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }
    }
}
