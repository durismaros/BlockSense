using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class ActivityLogService : IActivityLogService
    {
        private readonly ILogger<ActivityLogService> _logger;
        private readonly IApiClient _apiClient;

        public ActivityLogService(
            ILogger<ActivityLogService> logger,
            IApiClient apiClient)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));
        }

        /// <inheritdoc/>
        public async Task<ActivityLogPageDto?> GetPageAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .GetAsync<ActivityLogPageDto>(
                    requestUri: $"/api/users/me/activity?page={page}&pageSize={pageSize}",
                    cancellationToken: cancellationToken);

            switch (response)
            {
                case ApiResult<ActivityLogPageDto>.Success success:
                    return success.Data;

                case ApiResult.Failure failure:
                    _logger.LogWarning("Failed to load activity logs: {Title}", failure.ProblemDetails.Title);
                    return null;

                default:
                    return null;

            }
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ActivityLogDto>> GetLatestAsync(ulong afterId, CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .GetAsync<IReadOnlyList<ActivityLogDto>>(
                    requestUri: $"/api/users/me/activity/latest?afterId={afterId}",
                    cancellationToken: cancellationToken);

            switch (response)
            {
                case ApiResult<IReadOnlyList<ActivityLogDto>>.Success success:
                    return success.Data;

                case ApiResult.Failure failure:
                    _logger.LogWarning("Failed to fetch activity logs after id: {Title}", failure.ProblemDetails.Title);
                    return Array.Empty<ActivityLogDto>();

                default:
                    return Array.Empty<ActivityLogDto>();
            }
        }
    }
}
