using BlockSense.Backend.Data;
using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Wallet;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace BlockSense.Backend.Services.Implementations
{
    /// <summary>
    /// Provides exchange rate retrieval with in-memory caching to reduce redundant API calls.
    /// </summary>
    public sealed class ExchangeRateService : IExchangeRateService
    {
        private readonly ConcurrentDictionary<string, ExchangeRateResponse> _cache = new();
        private readonly CryptoApiClient _cryptoApiClient;
        private readonly CryptoConfig _cryptoConfig;

        /// <summary>
        /// Initializes a new instance of <see cref="ExchangeRateService"/> with required dependencies.
        /// </summary>
        /// <param name="cryptoApiClient">The HTTP client used to communicate with the crypto API.</param>
        /// <param name="cryptoConfig">The configuration containing exchange rate cache duration settings.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public ExchangeRateService(CryptoApiClient cryptoApiClient, IOptions<CryptoConfig> cryptoConfig)
        {
            _cryptoApiClient = cryptoApiClient
                ?? throw new ArgumentNullException(nameof(cryptoApiClient));

            _cryptoConfig = cryptoConfig.Value
                ?? throw new ArgumentNullException(nameof(cryptoConfig));
        }

        /// <inheritdoc/>
        public async Task<ExchangeRateResponse?> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
        {
            var cacheKey = BuildCacheKey(from, to);

            if (_cache.TryGetValue(cacheKey, out var cached) && !IsCacheExpired(cached))
                return cached;

            return await FetchAndCacheRateAsync(cacheKey, from, to, cancellationToken);
        }

        private async Task<ExchangeRateResponse?> FetchAndCacheRateAsync(
            string cacheKey,
            string from,
            string to,
            CancellationToken cancellationToken)
        {
            var path = $"market-data/exchange-rates/by-symbol/{from}/{to}";
            var response = await _cryptoApiClient.GetAsync<ExchangeRateEnvelope>(path, cancellationToken);

            var rate = MapToResponse(response.Data.Item);
            _cache[cacheKey] = rate;

            return rate;
        }

        private static ExchangeRateResponse MapToResponse(ExchangeRateItem item) => new()
        {
            FromAssetId = item.FromAssetId,
            FromAssetSymbol = item.FromAssetSymbol,
            Rate = ParseDecimal(item.Rate),
            ToAssetId = item.ToAssetId,
            ToAssetSymbol = item.ToAssetSymbol,
            CachedAt = DateTimeOffset.FromUnixTimeSeconds(item.CalculationTimestamp).UtcDateTime
        };

        private static string BuildCacheKey(string from, string to) =>
            $"{from}:{to}".ToUpperInvariant();

        private bool IsCacheExpired(ExchangeRateResponse cached) =>
            DateTime.UtcNow - cached.CachedAt > _cryptoConfig.ExchangeCacheDuration;

        private static decimal ParseDecimal(string? value) =>
            decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result) ? result : 0m;

        private sealed class ExchangeRateEnvelope
        {
            public required ExchangeRateData Data { get; set; }
        }

        private sealed class ExchangeRateData
        {
            public required ExchangeRateItem Item { get; set; }
        }

        private sealed class ExchangeRateItem
        {
            public required string FromAssetId { get; set; }
            public required string FromAssetSymbol { get; set; }
            public required string Rate { get; set; }
            public required string ToAssetId { get; set; }
            public required string ToAssetSymbol { get; set; }
            public required long CalculationTimestamp { get; set; }
        }
    }
}