using BlockSense.Backend.Data;
using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Models.Crypto;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Contracts.Enums;
using Microsoft.Extensions.Options;

namespace BlockSense.Backend.Services.Implementations
{
    /// <summary>
    /// Provides Ethereum-specific cryptocurrency service operations.
    /// </summary>
    public sealed class EthereumService : ICryptoService
    {
        private readonly CryptoApiClient _cryptoApiClient;
        private readonly CryptoConfig _cryptoConfig;

        /// <summary>
        /// Initializes a new instance of <see cref="EthereumService"/> with required dependencies.
        /// </summary>
        /// <param name="cryptoApiClient">The HTTP client used to communicate with the crypto API.</param>
        /// <param name="cryptoConfig">The configuration containing network and API settings.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public EthereumService(CryptoApiClient cryptoApiClient, IOptions<CryptoConfig> cryptoConfig)
        {
            _cryptoApiClient = cryptoApiClient
                ?? throw new ArgumentNullException(nameof(cryptoApiClient));

            _cryptoConfig = cryptoConfig.Value
                ?? throw new ArgumentNullException(nameof(cryptoConfig));
        }

        /// <inheritdoc/>
        public async Task<WalletBalanceResponse> GetBalanceAsync(string address, CancellationToken cancellationToken = default)
        {
            var path = $"addresses-latest/evm/ethereum/{_cryptoConfig.Ethereum.Network}/{address}/balance";
            var response = await _cryptoApiClient.GetAsync<BalanceEnvelope>(path, cancellationToken);

            return new WalletBalanceResponse
            {
                Address = address,
                Balance = ParseDecimal(response.Data.Item.ConfirmedBalance.Amount),
                Currency = "ETH"
            };
        }

        /// <inheritdoc/>
        public async Task<NextNonceResponse> GetNextAvailableNonce(string address, CancellationToken cancellationToken = default)
        {
            var path = $"addresses-latest/evm/ethereum/{_cryptoConfig.Ethereum.Network}/{address}/next-available-nonce";
            var response = await _cryptoApiClient.GetAsync<NextNonceEnvelope>(path, cancellationToken);

            return new NextNonceResponse
            {
                NextAvailableNonce = response.Data.Item.NextAvailableNonce
            };
        }

        /// <inheritdoc/>
        public async Task<TransactionListResponse> GetTransactionsAsync(string address, CancellationToken cancellationToken = default)
        {
            var path = $"addresses-latest/evm/ethereum/{_cryptoConfig.Ethereum.Network}/{address}/transactions?limit=5";
            var response = await _cryptoApiClient.GetAsync<EthTxListEnvelope>(path, cancellationToken);

            var transactions = response.Data.Items.Select(MapTransaction);

            return new TransactionListResponse
            {
                Address = address,
                Total = transactions.Count(),
                Transactions = transactions
            };
        }

        /// <inheritdoc/>
        public async Task<BroadcastTransactionResponse> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var path = $"/broadcast-transactions/ethereum/{_cryptoConfig.Ethereum.Network}";
            var body = BuildBroadcastBody(request.SignedTransactionHex);

            var response = await _cryptoApiClient.PostAsync<BroadcastEnvelope>(path, body, cancellationToken);

            return new BroadcastTransactionResponse
            {
                TransactionId = response.Data.Item.TransactionId
            };
        }

        private static object BuildBroadcastBody(string signedTransactionHex) => new
        {
            data = new
            {
                item = new { signedTransactionHex }
            }
        };

        private static TransactionDto MapTransaction(EthTxItem tx) => new()
        {
            TxHash = tx.Hash,
            Fee = ParseDecimal(tx.Fee.Amount),
            FromAddress = tx.Sender ?? "Unknown",
            ToAddress = tx.Recipient ?? "Unknown",
            Amount = ParseDecimal(tx.Value.Amount),
            Currency = "ETH",
            Status = ResolveStatus(tx.Status),
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(tx.Timestamp).UtcDateTime
        };

        private static TransactionStatus ResolveStatus(string? rawStatus) => rawStatus switch
        {
            "0x1" or "success" => TransactionStatus.Confirmed,
            "0x0" or "failed" => TransactionStatus.Failed,
            _ => TransactionStatus.Pending
        };

        private static decimal ParseDecimal(string? value) =>
            decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result) ? result : 0m;
    }
}