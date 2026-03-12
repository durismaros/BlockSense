using BlockSense.Backend.Data;
using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Exceptions.Generic;
using BlockSense.Backend.Models.Crypto;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Contracts.Enums;
using Microsoft.Extensions.Options;

namespace BlockSense.Backend.Services.Implementations
{
    /// <summary>
    /// Provides Bitcoin-specific cryptocurrency service operations.
    /// </summary>
    public sealed class BitcoinService : ICryptoService
    {
        private readonly CryptoApiClient _cryptoApiClient;
        private readonly CryptoConfig _cryptoConfig;

        /// <summary>
        /// Initializes a new instance of <see cref="BitcoinService"/> with required dependencies.
        /// </summary>
        /// <param name="cryptoApiClient">The HTTP client used to communicate with the crypto API.</param>
        /// <param name="cryptoConfig">The configuration containing network and API settings.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public BitcoinService(CryptoApiClient cryptoApiClient, IOptions<CryptoConfig> cryptoConfig)
        {
            _cryptoApiClient = cryptoApiClient
                ?? throw new ArgumentNullException(nameof(cryptoApiClient));

            _cryptoConfig = cryptoConfig.Value
                ?? throw new ArgumentNullException(nameof(cryptoConfig));
        }

        /// <inheritdoc/>
        public async Task<WalletBalanceResponse> GetBalanceAsync(string address, CancellationToken cancellationToken = default)
        {
            var path = $"addresses-latest/utxo/bitcoin/{_cryptoConfig.Bitcoin.Network}/{address}/balance";
            var response = await _cryptoApiClient.GetAsync<BalanceEnvelope>(path, cancellationToken);

            return new WalletBalanceResponse
            {
                Address = address,
                Balance = ParseDecimal(response.Data.Item.ConfirmedBalance.Amount),
                Currency = "BTC"
            };
        }

        /// <inheritdoc/>
        public Task<NextNonceResponse> GetNextAvailableNonce(string address, CancellationToken cancellationToken = default)
        {
            throw new NotFoundException();
        }

        /// <inheritdoc/>
        public async Task<TransactionListResponse> GetTransactionsAsync(string address, CancellationToken cancellationToken = default)
        {
            var confirmedTransactions = await FetchConfirmedTransactionsAsync(address, cancellationToken);
            var unconfirmedTransactions = await FetchUnconfirmedTransactionsAsync(address, cancellationToken);

            var transactions = MapAllTransactions(confirmedTransactions, unconfirmedTransactions, address);
            var utxos = ExtractUtxos(confirmedTransactions.Concat(unconfirmedTransactions), address);

            return new TransactionListResponse
            {
                Address = address,
                Total = transactions.Count(),
                Transactions = transactions,
                Utxos = utxos
            };
        }

        /// <inheritdoc/>
        public async Task<BroadcastTransactionResponse> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var path = $"/broadcast-transactions/bitcoin/{_cryptoConfig.Bitcoin.Network}";
            var body = BuildBroadcastBody(request.SignedTransactionHex);

            var response = await _cryptoApiClient.PostAsync<BroadcastEnvelope>(path, body, cancellationToken);

            return new BroadcastTransactionResponse
            {
                TransactionId = response.Data.Item.TransactionId
            };
        }

        private async Task<IEnumerable<BtcTxItem>> FetchConfirmedTransactionsAsync(string address, CancellationToken cancellationToken)
        {
            var path = $"addresses-latest/utxo/bitcoin/{_cryptoConfig.Bitcoin.Network}/{address}/transactions?limit=5";
            var response = await _cryptoApiClient.GetAsync<BtcTxListEnvelope>(path, cancellationToken);
            return response.Data.Items;
        }

        private async Task<IEnumerable<BtcTxItem>> FetchUnconfirmedTransactionsAsync(string address, CancellationToken cancellationToken)
        {
            var path = $"addresses-latest/utxo/bitcoin/{_cryptoConfig.Bitcoin.Network}/{address}/unconfirmed-transactions?limit=5";
            var response = await _cryptoApiClient.GetAsync<BtcTxListEnvelope>(path, cancellationToken);
            return response.Data.Items;
        }

        private static IEnumerable<TransactionDto> MapAllTransactions(
            IEnumerable<BtcTxItem> confirmed,
            IEnumerable<BtcTxItem> unconfirmed,
            string address)
        {
            return confirmed
                .Select(tx => MapTransaction(tx, address, TransactionStatus.Confirmed))
                .Concat(unconfirmed.Select(tx => MapTransaction(tx, address, TransactionStatus.Pending)))
                .OrderByDescending(tx => tx.Timestamp);
        }

        private static object BuildBroadcastBody(string signedTransactionHex) => new
        {
            data = new
            {
                item = new { signedTransactionHex }
            }
        };

        private static TransactionDto MapTransaction(BtcTxItem tx, string walletAddress, TransactionStatus status)
        {
            var received = SumOutputsToAddress(tx, walletAddress);
            var sent = SumInputsFromAddress(tx, walletAddress);
            var amount = received - sent;

            return new TransactionDto
            {
                TxHash = tx.Hash ?? tx.Id,
                Fee = ParseDecimal(tx.Fee?.Amount ?? "0"),
                FromAddress = ResolveFromAddress(tx, walletAddress),
                ToAddress = ResolveToAddress(tx, walletAddress),
                Amount = amount,
                Currency = "BTC",
                Status = status,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(tx.Timestamp).UtcDateTime
            };
        }

        private static decimal SumOutputsToAddress(BtcTxItem tx, string address) =>
            tx.Outputs?
                .Where(o => o.Addresses?.Contains(address) == true)
                .Sum(o => ParseDecimal(o.Value?.Amount ?? "0")) ?? 0m;

        private static decimal SumInputsFromAddress(BtcTxItem tx, string address) =>
            tx.Inputs?
                .Where(i => i.Addresses?.Contains(address) == true)
                .Sum(i => ParseDecimal(i.Value?.Amount ?? "0")) ?? 0m;

        private static string ResolveFromAddress(BtcTxItem tx, string walletAddress) =>
            tx.Inputs?.FirstOrDefault(i => i.Addresses?.Contains(walletAddress) == true)?.Addresses?.First()
                ?? tx.Inputs?.FirstOrDefault()?.Addresses?.FirstOrDefault()
                ?? "Unknown";

        private static string ResolveToAddress(BtcTxItem tx, string walletAddress) =>
            tx.Outputs?.FirstOrDefault(o => o.Addresses?.Contains(walletAddress) == false)?.Addresses?.FirstOrDefault()
                ?? tx.Outputs?.FirstOrDefault()?.Addresses?.FirstOrDefault()
                ?? "Unknown";

        private static List<UtxoDto> ExtractUtxos(IEnumerable<BtcTxItem> transactions, string address)
        {
            var txList = transactions.ToList();

            var unspentOutputs = txList
                .SelectMany(tx => tx.Outputs
                    .Select((output, index) => (tx.Hash, Index: index, Output: output))
                    .Where(x => x.Output.Addresses?.Contains(address) == true && !x.Output.IsSpent)
                    .Select(x => new UtxoDto
                    {
                        TransactionId = x.Hash,
                        OutputIndex = x.Index,
                        Amount = ParseDecimal(x.Output.Value?.Amount ?? "0")
                    }));

            var spentKeys = txList
                .SelectMany(tx => tx.Inputs
                    .Where(i => i.Addresses?.Contains(address) == true)
                    .Select(i => (i.TransactionId, i.OutputIndex)))
                .ToHashSet();

            return unspentOutputs
                .Where(u => !spentKeys.Contains((u.TransactionId, u.OutputIndex)))
                .ToList();
        }

        private static decimal ParseDecimal(string? value) =>
            decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result) ? result : 0m;
    }
}