using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IActivityLogRepository
    {
        Task InsertAsync(ActivityLogEntity entity, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLogEntity>> GetByUserAsync(uint userId, int limit = 50, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLogEntity>> GetBySystemAsync(int limit = 50, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLogEntity>> GetByCronAsync(int limit = 50, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLogEntity>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default);
    }
}
