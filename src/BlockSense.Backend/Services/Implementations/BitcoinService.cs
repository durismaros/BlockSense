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

        public async Task<WalletBalanceResponse> GetBalanceAsync(string address)
        {
            var path = $"addresses-latest/utxo/bitcoin/{_cryptoConfig.Bitcoin.Network}/{address}/balance";
            var response = await _cryptoApiClient.GetAsync<BalanceEnvelope>(path);
            var balance = response.Data.Item.ConfirmedBalance.Amount;

            return new WalletBalanceResponse
            {
                Address = address,
                Balance = ParseDecimal(balance),
                Currency = "BTC"
            };
        }

        public async Task<TransactionListResponse> GetTransactionsAsync(string address)
        {
            var path = $"addresses-latest/utxo/bitcoin/{_cryptoConfig.Bitcoin.Network}/{address}/transactions" +
                         $"?limit=5";

            var response = await _cryptoApiClient.GetAsync<TxListEnvelope>(path);
            var data = response.Data;
            var transactions = data.Items.Select(t => MapTransaction(t, address));

            return new TransactionListResponse
            {
                Address = address,
                Total = transactions.Count(),
                Transactions = transactions
            };
        }

        public async Task<BroadcastTransactionResponse> BroadcastAsync(BroadcastTransactionRequest request)
        {
            var path = $"transactions/utxo/bitcoin/{_cryptoConfig.Bitcoin.Network}/broadcast";
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

            var response = await _cryptoApiClient.PostAsync<BroadcastEnvelope>(path, body);

            return new BroadcastTransactionResponse
            {
                TransactionId = response.Data.Item.TransactionId
            };
        }

        private static TransactionDto MapTransaction(TxItem tx, string wallet)
        {
            var received = tx.Recipients?
                .Where(r => r.Address == wallet)
                .Sum(r => ParseDecimal(r.Value.Amount)) ?? 0m;

            var sent = tx.Senders?
                .Where(s => s.Address == wallet)
                .Sum(s => ParseDecimal(s.Value.Amount)) ?? 0m;

            var amount = received - sent;

            return new TransactionDto
            {
                TxHash = tx.Hash ?? tx.Id,
                Fee = ParseDecimal(tx.Fee.Amount),
                FromAddress = tx.Senders?.FirstOrDefault()?.Address ?? "Unknown",
                ToAddress = tx.Recipients?.FirstOrDefault()?.Address ?? "Unknown",
                Amount = amount,
                Currency = "BTC",
                Status = TransactionStatus.Confirmed,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(tx.Timestamp).UtcDateTime
            };
        }

        private static decimal ParseDecimal(string? value)
            => decimal.TryParse(value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result) ? result : 0m;



        private sealed class TxListEnvelope
        {
            public required TxListData Data { get; set; }
        }

        private sealed class TxListData
        {
            public required List<TxItem> Items { get; set; }
        }

        private sealed class TxItem
        {
            public required string Id { get; set; }
            public required string Hash { get; set; }
            public required AmountValue Fee { get; set; }
            public required List<TxParty> Senders { get; set; }
            public required List<TxParty> Recipients { get; set; }
            public required long Timestamp { get; set; }
        }

        private sealed class TxParty
        {
            public required string Address { get; set; }
            public required AmountValue Value { get; set; }
        }
    }
}
