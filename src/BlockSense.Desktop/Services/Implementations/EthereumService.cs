using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Services.Interfaces;
using NBitcoin;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class EthereumService : IEthereumService
    {
        private const string DerivationPath = "m/44'/60'/0'/0/0";

        private readonly IApiClient _apiClient;

        public EthereumService(IApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<WalletBalanceResponse?> GetBalanceAsync(string address, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<WalletBalanceResponse>($"api/ethereum/{address}/balance", cancellationToken);

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
                .GetAsync<TransactionListResponse>($"api/ethereum/{address}/transactions", cancellationToken);

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
                .PostAsync<BroadcastTransactionRequest, BroadcastTransactionResponse>("api/ethereum/broadcast", request, cancellationToken);

            if (result.IsSuccess && result is ApiResult<BroadcastTransactionResponse>.Success success)
            {
                return success.Data;
            }

            return null;
        }

        public string DeriveAddress(byte[] seed)
        {
            var privateKeyBytes = DerivePrivateKey(seed);
            try
            {
                var signer = new EthECKey(privateKeyBytes, isPrivate: true);
                return signer.GetPublicAddress();
            }
            finally
            {
                CryptographicOperations.ZeroMemory(privateKeyBytes);
            }
        }

        public string SignTransaction(EthereumSignRequest request)
        {
            var privateKeyBytes = DerivePrivateKey(request.Seed);

            try
            {
                var amountWei = ToWei(request.AmountEth);
                var gasPriceWei = GweiToWei(request.GasPriceGwei);

                var signer = new LegacyTransactionSigner();

                var signedHex = signer.SignTransaction(
                    privateKey: privateKeyBytes.ToHex(prefix: false),
                    chainId: new BigInteger(request.ChainId),
                    to: request.ToAddress,
                    amount: amountWei,
                    nonce: new BigInteger(request.Nonce),
                    gasPrice: gasPriceWei,
                    gasLimit: new BigInteger(request.GasLimit),
                    data: string.Empty);

                return signedHex;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(privateKeyBytes);
            }
        }

        public async Task<long?> GetNextAvailableNonce(string address, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<NextNonceResponse>($"api/ethereum/{address}/next-available-nonce", cancellationToken);

            if (result.IsSuccess && result is ApiResult<NextNonceResponse>.Success success)
            {
                return success.Data.NextAvailableNonce;
            }

            return null;
        }

        private static byte[] DerivePrivateKey(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(new KeyPath(DerivationPath));
            return derived.PrivateKey.ToBytes();
        }

        private static BigInteger ToWei(decimal eth)
            => new(decimal.ToDouble(eth * 1_000_000_000_000_000_000m));

        private static BigInteger GweiToWei(decimal gwei)
            => new(decimal.ToDouble(gwei * 1_000_000_000m));
    }
}
