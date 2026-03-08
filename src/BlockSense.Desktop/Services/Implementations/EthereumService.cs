using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using NBitcoin;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class EthereumService : IEthereumService
    {
        private readonly IApiClient _apiClient;
        private readonly ICurrentWalletProvider _currentWalletProvider;
        private readonly IEthereumProvider _ethereumProvider;
        private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;

        public EthereumService(
            IApiClient apiClient,
            ICurrentWalletProvider currentWalletProvider,
            IEthereumProvider ethereumProvider)
        {
            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));

            _currentWalletProvider = currentWalletProvider
                ?? throw new ArgumentNullException(nameof(currentWalletProvider));

            _ethereumProvider = ethereumProvider
                ?? throw new ArgumentNullException(nameof(ethereumProvider));

            _pinEntrySlidingPanel = MainWindow.Instance.PinEntrySlidingPanel
                ?? throw new ArgumentNullException(nameof(PinEntrySlidingPanel));
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

        public async Task<ExchangeRateResponse?> GetExchangeRateAsync(string toAssetSymbol, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<ExchangeRateResponse>($"/api/crypto/exchange-rate/ETH/{toAssetSymbol}", cancellationToken);

            if (result.IsSuccess && result is ApiResult<ExchangeRateResponse>.Success success)
            {
                return success.Data;
            }


            return null;
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

        public void Initialize(byte[] seed)
        {
            var address = DeriveAddress(seed);
            _ethereumProvider.Initialize(address);
        }

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_ethereumProvider.Address))
            {
                return;
            }

            var balance = await GetBalanceAsync(_ethereumProvider.Address, cancellationToken);
            var rate = await GetExchangeRateAsync("EUR", cancellationToken);
            var txs = await GetTransactionsAsync(_ethereumProvider.Address, cancellationToken);

            if (balance is null || rate is null || txs is null)
                return;

            _ethereumProvider.Set(
                balance.Balance,
                rate.Rate,
                txs.Transactions.ToList().AsReadOnly());
        }

        public async Task SignAndBroadcastAsync(
            string toAddress,
            decimal amount,
            CancellationToken cancellationToken = default)
        {
            byte[]? seed = null;

            _pinEntrySlidingPanel.ShowPanel(async pin =>
            {
                var decryptedSeed = _currentWalletProvider.DecryptSeed(pin);

                if (decryptedSeed is null)
                {
                    await _pinEntrySlidingPanel.ShowErrorState();
                    return;
                }

                seed = decryptedSeed;
            });

            if (seed is null)
            {
                return;
            }

            var nonce = await GetNextAvailableNonce(_ethereumProvider.Address, cancellationToken);

            if (nonce is null)
            {
                return;
            }

            try
            {
                var signedHex = SignTransaction(new EthereumSignRequest
                {
                    Seed = seed,
                    ToAddress = toAddress,
                    AmountEth = amount,
                    Nonce = nonce.Value,
                    GasPriceGwei = EthereumFees.DefaultGasPriceGwei,
                    GasLimit = EthereumFees.DefaultGasLimit,
                    ChainId = EthereumChain.CurrentNetwork
                });

                var result = await BroadcastAsync(
                    new BroadcastTransactionRequest { SignedTransactionHex = signedHex },
                    cancellationToken);

                MainWindow.Instance.ShowNotification(
                    "Transaction Broadcast",
                    $"Transaction {result?.TransactionId} broadcasted");
            }
            finally
            {
                CryptographicOperations.ZeroMemory(seed);
            }
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

        private static byte[] DerivePrivateKey(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(EthereumChain.DerivationPath);
            return derived.PrivateKey.ToBytes();
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

        private static BigInteger ToWei(decimal eth)
            => new(decimal.ToDouble(eth * 1_000_000_000_000_000_000m));

        private static BigInteger GweiToWei(decimal gwei)
            => new(decimal.ToDouble(gwei * 1_000_000_000m));
    }
}
