using BlockSense.Contracts.DTOs.Wallet;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateResponse?> GetRateAsync(string from, string to, CancellationToken cancellationToken = default);
    }
}
