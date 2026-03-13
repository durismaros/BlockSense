using BlockSense.Backend.Entities;
using BlockSense.Backend.Models.ActivityLog;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Backend.Utilities;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Contracts.Enums;
using System.Text.Json;

namespace BlockSense.Backend.Services.Implementations
{
    /// <summary>
    /// Provides operations for retrieving user activity logs.
    /// </summary>
    public sealed class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _activityLogRepository;

        /// <summary>
        /// Initializes a new instance of <see cref="ActivityLogService"/> with required dependencies.
        /// </summary>
        /// <param name="activityLogRepository">The repository for activity log entity operations.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
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
            var entries = await _activityLogRepository
                .GetPagedByUserIdAsync(userId, page, pageSize, cancellationToken);

            var totalCount = await _activityLogRepository
                .CountByUserIdAsync(userId, cancellationToken);

            return new ActivityLogPageDto
            {
                Entries = entries.Select(MapToDto).ToList().AsReadOnly(),
                TotalCount = totalCount,
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

            return logs.Select(MapToDto).ToList().AsReadOnly();
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

        /// <inheritdoc/>
        public async Task CreateAsync(
            ActivityType type,
            uint userId,
            string action,
            ActivityLogContext? context = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action must be provided.", nameof(action));

            var log = new ActivityLog
            {
                Id = default,
                Type = type,
                UserId = userId,
                Action = action,
                Context = context?.ToJson(),
                OccurredAt = DateTime.UtcNow
            };

            await _activityLogRepository.InsertAsync(log, cancellationToken);
        }
    }
}