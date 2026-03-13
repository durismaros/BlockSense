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
    /// <summary>
    /// Implements <see cref="IEthereumService"/> to manage Ethereum wallet operations,
    /// including balance retrieval, transaction history, signing, and broadcasting.
    /// </summary>
    public sealed class EthereumService : IEthereumService
    {
        private readonly IApiClient _apiClient;
        private readonly ICurrentWalletProvider _currentWalletProvider;
        private readonly IEthereumProvider _ethereumProvider;
        private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;

        /// <summary>
        /// Initializes a new instance of <see cref="EthereumService"/>.
        /// </summary>
        /// <param name="apiClient">The API client used to communicate with the backend.</param>
        /// <param name="currentWalletProvider">The provider for accessing the currently loaded wallet.</param>
        /// <param name="ethereumProvider">The provider for accessing and updating Ethereum wallet state.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
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

        /// <inheritdoc/>
        public void Initialize(byte[] seed)
        {
            var address = DeriveAddress(seed);
            _ethereumProvider.Initialize(address);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<WalletBalanceResponse?> GetBalanceAsync(string address, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<WalletBalanceResponse>($"api/ethereum/{address}/balance", cancellationToken);

            return ExtractData<WalletBalanceResponse>(result);
        }

        /// <inheritdoc/>
        public async Task<TransactionListResponse?> GetTransactionsAsync(string address, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<TransactionListResponse>($"api/ethereum/{address}/transactions", cancellationToken);

            return ExtractData<TransactionListResponse>(result);
        }

        /// <inheritdoc/>
        public async Task<ExchangeRateResponse?> GetExchangeRateAsync(string toAssetSymbol, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<ExchangeRateResponse>($"api/crypto/exchange-rate/ETH/{toAssetSymbol}", cancellationToken);

            return ExtractData<ExchangeRateResponse>(result);
        }

        /// <inheritdoc/>
        public async Task<long?> GetNextAvailableNonceAsync(string address, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<NextNonceResponse>($"api/ethereum/{address}/next-available-nonce", cancellationToken);

            return ExtractData<NextNonceResponse>(result)?.NextAvailableNonce;
        }

        /// <inheritdoc/>
        public async Task<BroadcastTransactionResponse?> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .PostAsync<BroadcastTransactionRequest, BroadcastTransactionResponse>(
                    "api/ethereum/broadcast", request, cancellationToken);

            return ExtractData<BroadcastTransactionResponse>(result);
        }

        /// <inheritdoc/>
        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_ethereumProvider.Address))
            {
                return;
            }

            var balance = await GetBalanceAsync(_ethereumProvider.Address, cancellationToken);
            var rate = await GetExchangeRateAsync("EUR", cancellationToken);
            var transactions = await GetTransactionsAsync(_ethereumProvider.Address, cancellationToken);

            if (balance is null || rate is null || transactions is null)
            {
                return;
            }

            _ethereumProvider.Set(
                balance.Balance,
                rate.Rate,
                transactions.Transactions.ToList().AsReadOnly());
        }

        /// <inheritdoc/>
        public Task SignAndBroadcastAsync(string toAddress, decimal amount, CancellationToken cancellationToken = default)
        {
            _pinEntrySlidingPanel.ShowPanel(async pin =>
                await ExecuteSignAndBroadcastAsync(pin, toAddress, amount, cancellationToken));

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public string SignTransaction(EthereumSignRequest request)
        {
            var privateKeyBytes = DerivePrivateKey(request.Seed);

            try
            {
                return BuildAndSignTransaction(privateKeyBytes, request);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(privateKeyBytes);
            }
        }

        private async Task ExecuteSignAndBroadcastAsync(
            string pin,
            string toAddress,
            decimal amount,
            CancellationToken cancellationToken)
        {
            var decryptedSeed = _currentWalletProvider.DecryptSeed(pin);

            if (decryptedSeed is null)
            {
                await _pinEntrySlidingPanel.ShowErrorState();
                return;
            }

            try
            {
                var nonce = await GetNextAvailableNonceAsync(_ethereumProvider.Address, cancellationToken);

                if (nonce is null)
                {
                    return;
                }

                var signedHex = SignTransaction(new EthereumSignRequest
                {
                    Seed = decryptedSeed,
                    ToAddress = toAddress,
                    AmountEth = amount,
                    Nonce = nonce.Value,
                    GasPriceGwei = EthereumFees.DefaultGasPriceGwei,
                    GasLimit = EthereumFees.DefaultGasLimit,
                    ChainId = EthereumChain.CurrentNetwork
                });

                await BroadcastAndNotifyAsync(signedHex, cancellationToken);
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification("Signing Error", ex.Message);
            }
            finally
            {
                _pinEntrySlidingPanel.HidePanel();
                CryptographicOperations.ZeroMemory(decryptedSeed);
            }
        }

        private async Task BroadcastAndNotifyAsync(string signedHex, CancellationToken cancellationToken)
        {
            var result = await BroadcastAsync(
                new BroadcastTransactionRequest { SignedTransactionHex = signedHex },
                cancellationToken);

            if (result is null)
            {
                MainWindow.Instance.ShowNotification(
                    "Broadcast Failed",
                    "Could not broadcast transaction. Please try again.");

                return;
            }

            MainWindow.Instance.ShowNotification(
                "Transaction Broadcast",
                $"Transaction {result.TransactionId} broadcasted successfully.");
        }

        private static string BuildAndSignTransaction(byte[] privateKeyBytes, EthereumSignRequest request)
        {
            var amountWei = ToWei(request.AmountEth);
            var gasPriceWei = GweiToWei(request.GasPriceGwei);

            var signer = new LegacyTransactionSigner();

            return signer.SignTransaction(
                privateKey: privateKeyBytes.ToHex(prefix: false),
                to: request.ToAddress,
                amount: amountWei,
                nonce: new BigInteger(request.Nonce),
                gasPrice: gasPriceWei,
                gasLimit: new BigInteger(request.GasLimit),
                data: string.Empty,
                chainId: new BigInteger(request.ChainId));
        }

        private static byte[] DerivePrivateKey(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(EthereumChain.DerivationPath);
            return derived.PrivateKey.ToBytes();
        }

        private static TData? ExtractData<TData>(ApiResult result) where TData : class
        {
            if (result.IsSuccess && result is ApiResult<TData>.Success success)
            {
                return success.Data;
            }

            return null;
        }

        private static BigInteger ToWei(decimal eth)
            => new(decimal.ToDouble(eth * 1_000_000_000_000_000_000m));

        private static BigInteger GweiToWei(decimal gwei)
            => new(decimal.ToDouble(gwei * 1_000_000_000m));
    }
}