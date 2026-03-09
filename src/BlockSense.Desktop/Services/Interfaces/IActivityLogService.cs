using BlockSense.Contracts.DTOs.User;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IActivityLogService
    {
        Task<ActivityLogPageDto?> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ActivityLogDto>> GetLatestAsync(ulong afterId, CancellationToken cancellationToken = default);
    }
}
