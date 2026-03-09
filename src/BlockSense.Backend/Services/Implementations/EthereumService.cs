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
    public sealed class EthereumService : ICryptoService
    {
        private readonly CryptoApiClient _cryptoApiClient;
        private readonly CryptoConfig _cryptoConfig;

        public EthereumService(CryptoApiClient cryptoApiClient, IOptions<CryptoConfig> cryptoConfig)
        {
            _cryptoApiClient = cryptoApiClient
                ?? throw new ArgumentNullException(nameof(cryptoApiClient));

            _cryptoConfig = cryptoConfig.Value
                ?? throw new ArgumentNullException(nameof(cryptoConfig));
        }

        public async Task<WalletBalanceResponse> GetBalanceAsync(string address, CancellationToken cancellationToken = default)
        {
            var path = $"addresses-latest/evm/ethereum/{_cryptoConfig.Ethereum.Network}/{address}/balance";
            var response = await _cryptoApiClient.GetAsync<BalanceEnvelope>(path, cancellationToken);
            var balance = response.Data.Item.ConfirmedBalance.Amount;

            return new WalletBalanceResponse
            {
                Address = address,
                Balance = ParseDecimal(balance),
                Currency = "ETH"
            };
        }

        public async Task<NextNonceResponse> GetNextAvailableNonce(string address, CancellationToken cancellationToken = default)
        {
            var path = $"addresses-latest/evm/ethereum/{_cryptoConfig.Ethereum.Network}/{address}/next-available-nonce";
            var response = await _cryptoApiClient.GetAsync<NextNonceEnvelope>(path, cancellationToken);
            var nonce = response.Data.Item.NextAvailableNonce;

            return new NextNonceResponse
            {
                NextAvailableNonce = nonce
            };
        }

        public async Task<TransactionListResponse> GetTransactionsAsync(string address, CancellationToken cancellationToken = default)
        {
            var path = $"addresses-latest/evm/ethereum/{_cryptoConfig.Ethereum.Network}/{address}/transactions" +
                         $"?limit=5";

            var response = await _cryptoApiClient.GetAsync<EthTxListEnvelope>(path, cancellationToken);
            var data = response.Data;
            var transactions = data.Items.Select(MapTransaction);

            return new TransactionListResponse
            {
                Address = address,
                Total = transactions.Count(),
                Transactions = transactions,
                Utxos = new List<UtxoDto>()
            };
        }

        public async Task<BroadcastTransactionResponse> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var path = $"/broadcast-transactions/ethereum/{_cryptoConfig.Ethereum.Network}";
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

        private static TransactionDto MapTransaction(EthTxItem tx)
        {
            var status = tx.Status switch
            {
                "0x1" or "success" => TransactionStatus.Confirmed,
                "0x0" or "failed" => TransactionStatus.Failed,
                _ => TransactionStatus.Pending
            };

            return new TransactionDto
            {
                TxHash = tx.Hash,
                Fee = ParseDecimal(tx.Fee.Amount),
                FromAddress = tx.Sender ?? "Unknown",
                ToAddress = tx.Recipient ?? "Unknown",
                Amount = ParseDecimal(tx.Value.Amount),
                Currency = "ETH",
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
