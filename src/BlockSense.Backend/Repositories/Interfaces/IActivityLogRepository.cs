using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IActivityLogRepository
    {
        Task InsertAsync(ActivityLog entity, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLog>> GetPagedByUserIdAsync(uint userId, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLog>> GetLatestAsync(uint userId, ulong afterId, CancellationToken cancellationToken = default);

        Task<ulong> CountByUserIdAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
