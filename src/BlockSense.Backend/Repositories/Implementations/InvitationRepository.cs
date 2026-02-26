using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
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
        public async Task<InvitationCode?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id              AS Id,
                    code            AS Code,
                    issued_to_id    AS IssuedToId,
                    redeemed_by_id  AS RedeemedById,
                    created_at      AS CreatedAt,
                    expires_at      AS ExpiresAt,
                    is_revoked      AS IsRevoked
                FROM invitation_codes
                WHERE id = @Id
                LIMIT 1
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.UInt32) { Value = id },
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
                    issued_to_id    AS IssuedToId,
                    redeemed_by_id  AS RedeemedById,
                    created_at      AS CreatedAt,
                    expires_at      AS ExpiresAt,
                    is_revoked      AS IsRevoked
                FROM invitation_codes
                WHERE code = @Code
                LIMIT 1
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Code", MySqlDbType.String, 32) { Value = code }
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
                    issued_to_id    AS IssuedToId,
                    redeemed_by_id  AS RedeemedById,
                    created_at      AS CreatedAt,
                    expires_at      AS ExpiresAt,
                    is_revoked      AS IsRevoked
                FROM invitation_codes
                WHERE code = @Code
                LIMIT 1
                FOR UPDATE;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Code", MySqlDbType.String, 32) { Value = code }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCode>(reader).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InvitationCode>> GetByIssuedToIdAsync(uint issuedToId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    ic.id               AS Id,
                    ic.code             AS Code,
                    ic.issued_to_id     AS IssuedToId,
                    ic.redeemed_by_id   AS RedeemedById,
                    u.username          AS RedeemedByUsername,
                    ic.created_at       AS CreatedAt,
                    ic.expires_at       AS ExpiresAt,
                    ic.is_revoked       AS IsRevoked
                FROM invitation_codes ic
                LEFT JOIN users u ON u.id = ic.redeemed_by_id
                WHERE ic.issued_to_id = @IssuedToId
                ORDER BY
                    ic.redeemed_by_id IS NULL   DESC,
                    ic.created_at               ASC
                """;

            var parameters = new[]
            {
                new MySqlParameter("@IssuedToId", MySqlDbType.UInt32) { Value = issuedToId },
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<InvitationCode>(reader).ToList();
        }

        /// <inheritdoc/>
        public async Task<uint> CreateAsync(InvitationCode invitationCode, CancellationToken cancellationToken = default)
        {
            const string sql = """
                INSERT INTO invitation_codes (
                    code,
                    issued_to_id,
                    redeemed_by_id,
                    created_at,
                    expires_at,
                    is_revoked )
                VALUES (
                    @Code,
                    @IssuedToId,
                    @RedeemedById,
                    @CreatedAt,
                    @ExpiresAt,
                    @IsRevoked );
                SELECT LAST_INSERT_ID();
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Code",         MySqlDbType.String, 32) { Value = invitationCode.Code },
                new MySqlParameter("@IssuedToId",   MySqlDbType.UInt32)     { Value = invitationCode.IssuedToId },
                new MySqlParameter("@RedeemedById", MySqlDbType.UInt32)     { Value = (object?)invitationCode.RedeemedById ?? DBNull.Value },
                new MySqlParameter("@CreatedAt",    MySqlDbType.DateTime)   { Value = invitationCode.CreatedAt },
                new MySqlParameter("@ExpiresAt",    MySqlDbType.DateTime)   { Value = invitationCode.ExpiresAt },
                new MySqlParameter("@IsRevoked",    MySqlDbType.Bit)        { Value = invitationCode.IsRevoked },
            };

            var insertId =
                await _databaseContext.ExecuteScalarAsync<ulong>(sql, parameters, cancellationToken);

            return Convert.ToUInt32(insertId);
        }

        /// <inheritdoc/>
        public async Task RedeemAsync(uint id, uint redeemedById, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE invitation_codes
                SET redeemed_by_id = @RedeemedById
                WHERE id = @Id
                  AND redeemed_by_id IS NULL
                  AND is_revoked = 0
                  AND expires_at > UTC_TIMESTAMP(6)
                """;
            
            var parameters = new[]
            {
                new MySqlParameter("@Id",           MySqlDbType.UInt32) { Value = id },
                new MySqlParameter("@RedeemedById", MySqlDbType.UInt32) { Value = redeemedById },
            };
            
            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RevokeAsync(uint id, CancellationToken cancellationToken = default)
        {
            const string sql = """
                UPDATE invitation_codes
                SET is_revoked = 1
                WHERE id = @Id
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.UInt32) { Value = id },
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }
    }
}
