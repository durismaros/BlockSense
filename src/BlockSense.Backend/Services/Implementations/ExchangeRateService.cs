using BlockSense.Backend.Data;
using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Wallet;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace BlockSense.Backend.Services.Implementations
{
    public sealed class ExchangeRateService : IExchangeRateService
    {
        private readonly ConcurrentDictionary<string, ExchangeRateResponse> _exchangeRateCache = new();
        private readonly CryptoApiClient _cryptoApiClient;
        private readonly CryptoConfig _cryptoConfig;

        public ExchangeRateService(CryptoApiClient cryptoApiClient, IOptions<CryptoConfig> cryptoConfig)
        {
            _cryptoApiClient = cryptoApiClient
                ?? throw new ArgumentNullException(nameof(cryptoApiClient));

            _cryptoConfig = cryptoConfig.Value
                ?? throw new ArgumentNullException(nameof(cryptoConfig));
        }

        public async Task<ExchangeRateResponse?> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
        {
            var key = $"{from}:{to}".ToUpperInvariant();

            if (_exchangeRateCache.TryGetValue(key, out var cached) && !IsExpired(cached))
            {
                return cached;
            }

            return await FetchAndCacheAsync(key, from, to, cancellationToken);
        }

        private async Task<ExchangeRateResponse?> FetchAndCacheAsync(string key, string from, string to, CancellationToken cancellationToken)
        {

            var path = $"market-data/exchange-rates/by-symbol/{from}/{to}";
            var response = await _cryptoApiClient.GetAsync<ExchangeRateEnvelope>(path);

            var exchangeRate = new ExchangeRateResponse
            {
                FromAssetId = response.Data.Items.FromAssetId,
                FromAssetSymbol = response.Data.Items.FromAssetSymbol,
                Rate = ParseDecimal(response.Data.Items.Rate),
                ToAssetId = response.Data.Items.ToAssetId,
                ToAssetSymbol = response.Data.Items.ToAssetSymbol,
                CachedAt = DateTimeOffset.FromUnixTimeSeconds(response.Data.Items.CalculationTimestamp).UtcDateTime
            };

            _exchangeRateCache[key] = exchangeRate;
            return exchangeRate;
        }

        private bool IsExpired(ExchangeRateResponse rate)
            => DateTime.UtcNow - rate.CachedAt > _cryptoConfig.ExchangeCacheDuration;

        private static decimal ParseDecimal(string? value)
            => decimal.TryParse(value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result) ? result : 0m;

        private sealed class ExchangeRateEnvelope
        {
            public required ExchangeRateData Data { get; set; }
        }

        private sealed class ExchangeRateData
        {
            public required ExchangeRateItem Items { get; set; }
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
