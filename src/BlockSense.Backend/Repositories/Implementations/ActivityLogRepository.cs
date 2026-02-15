using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
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

        public async Task InsertAsync(ActivityLogEntity activityLog, CancellationToken cancellationToken = default)
        {
            const string sqlQuery = """
                INSERT INTO activity_logs (
                    actor_type,
                    actor_id,
                    action,
                    context,
                    created_at)
                VALUES (
                    @ActorType,
                    @ActorId,
                    @Action,
                    @Context,
                    @CreatedAt);
                """;

            var parameters = new[]
            {
                new MySqlParameter("@ActorType", MySqlDbType.Enum) { Value = activityLog.ActorType },
                new MySqlParameter("@ActorId", MySqlDbType.UInt32) { Value = activityLog.ActorId },
                new MySqlParameter("@Action", MySqlDbType.VarChar, 255) { Value = activityLog.Action },
                new MySqlParameter("@Context", MySqlDbType.JSON) { Value = activityLog.Context },
                new MySqlParameter("@CreatedAt", MySqlDbType.DateTime) { Value = activityLog.CreatedAt }
            };

            var result = await _databaseContext.ExecuteNonQueryAsync(sqlQuery, parameters, cancellationToken);
        }
    }
}
