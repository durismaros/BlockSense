using BlockSense.Backend.Entities;
using BlockSense.Backend.Models;
using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface ITokenService
    {
        Task<RefreshTokenDto> CreateRefreshTokenAsync(uint userId, DeviceContext deviceContext, CancellationToken cancellationToken = default);
        Task<bool> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

        Task<AccessTokenDto> CreateAccessTokenAsync(UserEntity user, CancellationToken cancellationToken = default);
    }
}
