using BlockSense.Contracts.DTOs.User;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface IActivityLogService
    {
        Task<ActivityLogPageDto> GetPageAsync(uint userId, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLogDto>> GetLatestAsync(uint userId, ulong afterId, CancellationToken cancellationToken = default);
    }
}
