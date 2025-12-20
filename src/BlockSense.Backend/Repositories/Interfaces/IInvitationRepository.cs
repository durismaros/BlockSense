using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IInvitationRepository
    {
        Task<InvitationCodeEntity?> GetByIdAsync(uint invitationCodeId, CancellationToken cancellationToken = default);
        Task<InvitationCodeEntity?> GetByCodeAsync(string invitationCode, CancellationToken cancellationToken = default);

        Task<IEnumerable<InvitationCodeEntity>> GetByUserAsync(uint userId, CancellationToken cancellationToken = default);

        Task<bool> CodeExistsAsync(string invitationCode, CancellationToken cancellationToken = default);
        Task<bool> IsCodeActiveAsync(string invitationCode, CancellationToken cancellationToken = default);

        Task<uint> CreateAsync(InvitationCodeEntity invitation, CancellationToken cancellationToken = default);

        Task MarkAsUsedAsync(uint invitationCodeId, CancellationToken cancellationToken = default);
        Task RevokeAsync(uint invitationCodeId, CancellationToken cancellationToken = default);
    }
}
