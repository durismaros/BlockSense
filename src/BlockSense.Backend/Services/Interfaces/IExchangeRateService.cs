using BlockSense.Contracts.DTOs.Wallet;

namespace BlockSense.Backend.Services.Interfaces
{
    /// <summary>
    /// Defines operations for retrieving cryptocurrency exchange rates.
    /// </summary>
    public interface IExchangeRateService
    {
        /// <summary>
        /// Retrieves the exchange rate between two currency symbols.
        /// </summary>
        /// <param name="from">The source currency symbol (e.g., "BTC").</param>
        /// <param name="to">The target currency symbol (e.g., "USD").</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>
        /// An <see cref="ExchangeRateResponse"/> with the rate details, or <c>null</c> if unavailable.
        /// </returns>
        Task<ExchangeRateResponse?> GetRateAsync(string from, string to, CancellationToken cancellationToken = default);
    }
}