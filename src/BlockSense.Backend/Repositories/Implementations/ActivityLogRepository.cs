using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Repositories.Implementations
{
    public sealed class ActivityLogRepository
    {
        private readonly DatabaseContext _databaseContext;

        public ActivityLogRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
        }

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
                new MySqlParameter("@Type",       MySqlDbType.Enum)          { Value = log.Type.ToString().ToLowerInvariant() },
                new MySqlParameter("@UserId",     MySqlDbType.UInt32)        { Value = log.UserId },
                new MySqlParameter("@Action",     MySqlDbType.VarChar, 255)  { Value = log.Action },
                new MySqlParameter("@Context",    MySqlDbType.JSON)          { Value = (object?)log.Context ?? DBNull.Value },
                new MySqlParameter("@OccurredAt", MySqlDbType.DateTime)      { Value = log.OccurredAt }
            };

            await _databaseContext.ExecuteNonQueryAsync(sql, parameters, cancellationToken);
        }
    }
}
