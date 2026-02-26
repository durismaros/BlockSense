using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IInvitationRepository
    {
        Task<InvitationCode?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

        Task<InvitationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        Task<InvitationCode?> GetByCodeForUpdateAsync(string code, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<InvitationCode>> GetByIssuedToIdAsync(uint issuedToId, CancellationToken cancellationToken = default);

        Task<uint> CreateAsync(InvitationCode invitationCode, CancellationToken cancellationToken = default);

        Task RedeemAsync(uint id, uint redeemedById, CancellationToken cancellationToken = default);

        Task RevokeAsync(uint id, CancellationToken cancellationToken = default);


        //Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

        //Task<bool> IsActiveAsync(string code, CancellationToken cancellationToken = default);
    }
}
