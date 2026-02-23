using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IActivityLogRepository
    {
        Task InsertAsync(ActivityLog entity, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLog>> GetByUserAsync(uint userId, int limit = 50, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLog>> GetBySystemAsync(int limit = 50, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLog>> GetByCronAsync(int limit = 50, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLog>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default);
    }
}
