using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Services.Interfaces;
using NBitcoin;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class BitcoinService : IBitcoinService
    {
        private const string DerivationPath = "m/44'/0'/0'/0/0";

        private readonly IApiClient _apiClient;
        private readonly Network _network;

        public BitcoinService(IApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _network = Network.Main ?? throw new ArgumentNullException(nameof(Network));
        }

        public async Task<WalletBalanceResponse?> GetBalanceAsync(string address, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<WalletBalanceResponse>($"api/bitcoin/{address}/balance", cancellationToken);

            if (result.IsSuccess && result is ApiResult<WalletBalanceResponse>.Success success)
            {
                return success.Data;
            }

            return null;
        }

        public async Task<TransactionListResponse?> GetTransactionsAsync(string address, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<TransactionListResponse>($"api/bitcoin/{address}/transactions", cancellationToken);

            if (result.IsSuccess && result is ApiResult<TransactionListResponse>.Success success)
            {
                return success.Data;
            }

            return null;
        }

        public async Task<BroadcastTransactionResponse?> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .PostAsync<BroadcastTransactionRequest, BroadcastTransactionResponse>(
                    "api/bitcoin/broadcast", request, cancellationToken);

            if (result.IsSuccess && result is ApiResult<BroadcastTransactionResponse>.Success success)
            {
                return success.Data;
            }

            return null;
        }

        public string DeriveAddress(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(new KeyPath(DerivationPath));

            return derived.PrivateKey.PubKey
                .GetAddress(ScriptPubKeyType.Legacy, _network)
                .ToString();
        }

        public string SignTransaction(BitcoinSignRequest request)
        {
            var privateKeyBytes = DerivePrivateKey(request.Seed);

            try
            {
                var key = new Key(privateKeyBytes);

                var source = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, _network);
                var destination = BitcoinAddress.Create(request.ToAddress, _network);

                var fundingCoin = new Coin(
                    fromTxHash: uint256.Zero,
                    fromOutputIndex: 0,
                    amount: Money.Coins(request.BalanceBtc),
                    scriptPubKey: source.ScriptPubKey);

                var tx = _network.CreateTransactionBuilder()
                    .AddCoins(fundingCoin)
                    .AddKeys(key)
                    .Send(destination, Money.Coins(request.AmountBtc))
                    .SetChange(source)
                    .SendFees(Money.Coins(request.FeeBtc))
                    .BuildTransaction(sign: true);

                return tx.ToHex();
            }
            finally
            {
                CryptographicOperations.ZeroMemory(privateKeyBytes);
            }
        }

        private static byte[] DerivePrivateKey(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(new KeyPath(DerivationPath));
            return derived.PrivateKey.ToBytes();
        }
    }
}
