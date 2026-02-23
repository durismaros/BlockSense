using BlockSense.Backend.Entities;
using BlockSense.Contracts.DTOs.Invitation;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IInvitationRepository
    {
        Task<InvitationCode?> GetByIdAsync(uint invitationId, CancellationToken cancellationToken = default);

        Task<InvitationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        Task<InvitationCode?> GetByCodeForUpdateAsync(string code, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<InvitationCode>> GetByUserAsync(uint userId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<InvitationCode>> GetWithInviteeByUserAsync(uint userId, CancellationToken cancellationToken = default);

        Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

        Task<bool> IsActiveAsync(string code, CancellationToken cancellationToken = default);

        Task<uint> CreateAsync(InvitationCode invitation, CancellationToken cancellationToken = default);

        Task MarkAsUsedAsync(uint invitationId, uint usedByUserId, CancellationToken cancellationToken = default);

        Task RevokeAsync(uint invitationId, CancellationToken cancellationToken = default);
    }
}
