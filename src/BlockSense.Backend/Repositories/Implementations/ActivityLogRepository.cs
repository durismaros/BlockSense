using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    /// <summary>
    /// MySQL implementation of <see cref="IActivityLogRepository"/>.
    /// </summary>
    public sealed class ActivityLogRepository : IActivityLogRepository
    {
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of <see cref="ActivityLogRepository"/>.
        /// </summary>
        /// <param name="databaseContext">The database context used to execute queries.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="databaseContext"/> is null.</exception>
        public ActivityLogRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task InsertAsync(ActivityLog log, CancellationToken cancellationToken = default)
        {
            const string sql = """
                INSERT INTO activity_logs (
                    type,
                    user_id,
                    action,
                    context,
                    occurred_at )
                VALUES (
                    @Type,
                    @UserId,
                    @Action,
                    @Context,
                    @OccurredAt );
                """;

            var parameters = new[]
            {
                new MySqlParameter("@Type",       MySqlDbType.Enum)         { Value = log.Type.ToString().ToLowerInvariant() },
                new MySqlParameter("@UserId",     MySqlDbType.UInt32)       { Value = log.UserId },
                new MySqlParameter("@Action",     MySqlDbType.VarChar, 255) { Value = log.Action },
                new MySqlParameter("@Context",    MySqlDbType.JSON)         { Value = (object?)log.Context ?? DBNull.Value },
                new MySqlParameter("@OccurredAt", MySqlDbType.DateTime)     { Value = log.OccurredAt }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ActivityLog>> GetPagedByUserIdAsync(
            uint userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id          AS Id,
                    type        AS Type,
                    user_id     AS UserId,
                    action      AS Action,
                    context     AS Context,
                    occurred_at AS OccurredAt
                FROM activity_logs
                WHERE user_id = @UserId
                ORDER BY occurred_at DESC, id DESC
                LIMIT @PageSize OFFSET @Offset;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId",   MySqlDbType.UInt32) { Value = userId },
                new MySqlParameter("@PageSize", MySqlDbType.Int32)  { Value = pageSize },
                new MySqlParameter("@Offset",   MySqlDbType.Int32)  { Value = (page - 1) * pageSize }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<ActivityLog>(reader).AsList().AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ActivityLog>> GetLatestAsync(
            uint userId,
            ulong afterId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id          AS Id,
                    type        AS Type,
                    user_id     AS UserId,
                    action      AS Action,
                    context     AS Context,
                    occurred_at AS OccurredAt
                FROM activity_logs
                WHERE user_id = @UserId
                  AND id > @AfterId
                ORDER BY occurred_at DESC, id DESC;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId",  MySqlDbType.UInt32) { Value = userId },
                new MySqlParameter("@AfterId", MySqlDbType.UInt64) { Value = afterId }
            };

            await using var reader =
                await _databaseContext.ExecuteReaderAsync(sql, parameters, cancellationToken);

            return SqlMapper.Parse<ActivityLog>(reader).AsList().AsReadOnly();
        }

        /// <inheritdoc/>
        public async Task<ulong> CountByUserIdAsync(uint userId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM activity_logs
                WHERE user_id = @UserId;
                """;

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.UInt32) { Value = userId }
            };

            return await _databaseContext.ExecuteScalarAsync<ulong>(sql, parameters, cancellationToken);
        }
    }
}