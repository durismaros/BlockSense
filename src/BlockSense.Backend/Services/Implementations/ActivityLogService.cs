using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Backend.Utilities;
using BlockSense.Contracts.DTOs.User;

namespace BlockSense.Backend.Services.Implementations
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _activityLogRepository;

        public ActivityLogService(IActivityLogRepository activityLogRepository)
        {
            _activityLogRepository = activityLogRepository
                ?? throw new ArgumentNullException(nameof(activityLogRepository));
        }

        /// <inheritdoc/>
        public async Task<ActivityLogPageDto> GetPageAsync(
            uint userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var entriesTask = await _activityLogRepository
                .GetPagedByUserIdAsync(userId, page, pageSize, cancellationToken);

            var countTask = await _activityLogRepository
                .CountByUserIdAsync(userId, cancellationToken);

            return new ActivityLogPageDto
            {
                Entries = entriesTask.Select(MapToDto).ToList().AsReadOnly(),
                TotalCount = countTask,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ActivityLogDto>> GetLatestAsync(
            uint userId,
            ulong afterId,
            CancellationToken cancellationToken = default)
        {
            var logs = await _activityLogRepository
                .GetLatestAsync(userId, afterId, cancellationToken);

            return logs
                .Select(MapToDto)
                .ToList().AsReadOnly();
        }

        private static ActivityLogDto MapToDto(ActivityLog log) => new()
        {
            Id = log.Id,
            Type = log.Type,
            UserId = log.UserId,
            Action = log.Action,
            ActivityMessage = ActivityMessageMapper.Map(log.Action, log.Context),
            OccurredAt = log.OccurredAt
        };
    }
}
