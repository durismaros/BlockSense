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
    public sealed class BitcoinService : ICryptoService
    {
        private readonly CryptoApiClient _cryptoApiClient;
        private readonly CryptoConfig _cryptoConfig;

        public BitcoinService(CryptoApiClient cryptoApiClient, IOptions<CryptoConfig> cryptoConfig)
        {
            _cryptoApiClient = cryptoApiClient
                ?? throw new ArgumentNullException(nameof(cryptoApiClient));

            _cryptoConfig = cryptoConfig.Value
                ?? throw new ArgumentNullException(nameof(cryptoConfig));
        }

        public async Task<WalletBalanceResponse> GetBalanceAsync(string address, CancellationToken cancellationToken = default)
        {
            var path = $"addresses-latest/utxo/bitcoin/{_cryptoConfig.Bitcoin.Network}/{address}/balance";
            var response = await _cryptoApiClient.GetAsync<BalanceEnvelope>(path, cancellationToken);
            var balance = response.Data.Item.ConfirmedBalance.Amount;

            return new WalletBalanceResponse
            {
                Address = address,
                Balance = ParseDecimal(balance),
                Currency = "BTC"
            };
        }

        public async Task<NextNonceResponse> GetNextAvailableNonce(string address, CancellationToken cancellationToken = default)
        {
            throw new NotFoundException();
        }

        public async Task<TransactionListResponse> GetTransactionsAsync(string address, CancellationToken cancellationToken = default)
        {
            var confirmedPath = $"addresses-latest/utxo/bitcoin/{_cryptoConfig.Bitcoin.Network}/{address}/transactions" +
                $"?limit=5";

            var unconfirmedPath = $"addresses-latest/utxo/bitcoin/{_cryptoConfig.Bitcoin.Network}/{address}/unconfirmed-transactions" +
                $"?limit=5";

            var confirmedResponse = await _cryptoApiClient.GetAsync<BtcTxListEnvelope>(confirmedPath, cancellationToken);
            var confirmedTxs = confirmedResponse.Data.Items;

            var unconfirmedResponse = await _cryptoApiClient.GetAsync<BtcTxListEnvelope>(unconfirmedPath, cancellationToken);
            var unconfirmedTxs = unconfirmedResponse.Data.Items;

            var transactions = confirmedTxs
                .Select(tx => MapTransaction(tx, address, TransactionStatus.Confirmed))
                .Concat(unconfirmedTxs.Select(tx => MapTransaction(tx, address, TransactionStatus.Pending)))
                .OrderByDescending(tx => tx.Timestamp);

            var allTransactions = confirmedTxs.Concat(unconfirmedTxs).ToList();
            var utxos = ExtractUtxos(allTransactions, address);

            foreach (var x in utxos)
            {
                Console.WriteLine(x.TransactionId);
                Console.WriteLine(x.OutputIndex);
                Console.WriteLine(x.Amount);
            }

            return new TransactionListResponse
            {
                Address = address,
                Total = transactions.Count(),
                Transactions = transactions,
                Utxos = utxos
            };
        }

        public async Task<BroadcastTransactionResponse> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var path = $"/broadcast-transactions/bitcoin/{_cryptoConfig.Bitcoin.Network}";
            var body = new
            {
                data = new
                {
                    item = new
                    {
                        signedTransactionHex = request.SignedTransactionHex
                    }
                }
            };

            var response = await _cryptoApiClient.PostAsync<BroadcastEnvelope>(path, body, cancellationToken);

            return new BroadcastTransactionResponse
            {
                TransactionId = response.Data.Item.TransactionId
            };
        }

        private static TransactionDto MapTransaction(BtcTxItem tx, string wallet, TransactionStatus status)
        {
            var received = tx.Outputs?
                .Where(o => o.Addresses?.Contains(wallet) == true)
                .Sum(o => ParseDecimal(o.Value?.Amount ?? "0")) ?? 0m;

            var sent = tx.Inputs?
                .Where(i => i.Addresses?.Contains(wallet) == true)
                .Sum(i => ParseDecimal(i.Value?.Amount ?? "0")) ?? 0m;

            var amount = received - sent;

            var fromAddress = tx.Inputs?.FirstOrDefault(i =>
                i.Addresses?.Contains(wallet) == true)?.Addresses?.First()
                ?? tx.Inputs?.FirstOrDefault()?.Addresses?.FirstOrDefault()
                ?? "Unknown";

            var toAddress = tx.Outputs?.FirstOrDefault(o =>
                o.Addresses?.Contains(wallet) == false)?.Addresses?.FirstOrDefault()
                ?? tx.Outputs?.FirstOrDefault()?.Addresses?.FirstOrDefault()
                ?? "Unknown";

            return new TransactionDto
            {
                TxHash = tx.Hash ?? tx.Id,
                Fee = ParseDecimal(tx.Fee?.Amount ?? "0"),
                FromAddress = fromAddress,
                ToAddress = toAddress,
                Amount = amount,
                Currency = "BTC",
                Status = status,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(tx.Timestamp).UtcDateTime
            };
        }

        private static List<UtxoDto> ExtractUtxos(IEnumerable<BtcTxItem> transactions, string address)
        {
            var txList = transactions.ToList();

            // Build set of all spent outputs across all transactions
            var spentOutputs = txList
                .SelectMany(tx => tx.Inputs
                    .Where(i => i.TransactionId is not null)
                    .Select(i => (i.TransactionId!, i.OutputIndex)))
                .ToHashSet();

            // An output is a UTXO if it pays to our address, is not spent,
            // and is not referenced by any input in our transaction history
            return txList
                .SelectMany(tx => tx.Outputs
                    .Select((output, index) => (TxId: tx.Hash ?? tx.Id, Index: index, Output: output))
                    .Where(x =>
                        x.Output.Addresses?.Contains(address) == true &&
                        !x.Output.IsSpent &&
                        !spentOutputs.Contains((x.TxId, x.Index)))
                    .Select(x => new UtxoDto
                    {
                        TransactionId = x.TxId,
                        OutputIndex = x.Index,
                        Amount = ParseDecimal(x.Output.Value?.Amount ?? "0")
                    }))
                .ToList();
        }

        private static decimal ParseDecimal(string? value)
            => decimal.TryParse(value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result) ? result : 0m;
    }
}
