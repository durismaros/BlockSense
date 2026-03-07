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
            var confirmedTxs = confirmedResponse.Data.Item;

            var unconfirmedResponse = await _cryptoApiClient.GetAsync<BtcTxListEnvelope>(unconfirmedPath, cancellationToken);
            var unconfirmedTxs = unconfirmedResponse.Data.Item;

            var transactions = confirmedTxs
                .Select(tx => MapTransaction(tx, address, TransactionStatus.Confirmed))
                .Concat(unconfirmedTxs.Select(tx => MapTransaction(tx, address, TransactionStatus.Pending)))
                .OrderByDescending(tx => tx.Timestamp);

            return new TransactionListResponse
            {
                Address = address,
                Total = transactions.Count(),
                Transactions = transactions
            };
        }

        public async Task<BroadcastTransactionResponse> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default)
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

            var response = await _cryptoApiClient.PostAsync<BroadcastEnvelope>(path, body, cancellationToken);

            return new BroadcastTransactionResponse
            {
                TransactionId = response.Data.Item.TransactionId
            };
        }

        private static TransactionDto MapTransaction(BtcTxItem tx, string wallet, TransactionStatus status)
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
                Status = status,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(tx.Timestamp).UtcDateTime
            };
        }

        private static decimal ParseDecimal(string? value)
            => decimal.TryParse(value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result) ? result : 0m;
    }
}
