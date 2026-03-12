using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using NBitcoin;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    /// <summary>
    /// Implements <see cref="IBitcoinService"/> to manage Bitcoin wallet operations,
    /// including balance retrieval, transaction history, signing, and broadcasting.
    /// </summary>
    public sealed class BitcoinService : IBitcoinService
    {
        private readonly IApiClient _apiClient;
        private readonly ICurrentWalletProvider _currentWalletProvider;
        private readonly IBitcoinProvider _bitcoinProvider;
        private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;

        /// <summary>
        /// Initializes a new instance of <see cref="BitcoinService"/>.
        /// </summary>
        /// <param name="apiClient">The API client used to communicate with the backend.</param>
        /// <param name="currentWalletProvider">The provider for accessing the currently loaded wallet.</param>
        /// <param name="bitcoinProvider">The provider for accessing and updating Bitcoin wallet state.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
        public BitcoinService(
            IApiClient apiClient,
            ICurrentWalletProvider currentWalletProvider,
            IBitcoinProvider bitcoinProvider)
        {
            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));

            _currentWalletProvider = currentWalletProvider
                ?? throw new ArgumentNullException(nameof(currentWalletProvider));

            _bitcoinProvider = bitcoinProvider
                ?? throw new ArgumentNullException(nameof(bitcoinProvider));

            _pinEntrySlidingPanel = MainWindow.Instance.PinEntrySlidingPanel
                ?? throw new ArgumentNullException(nameof(PinEntrySlidingPanel));
        }

        /// <inheritdoc/>
        public void Initialize(byte[] seed)
        {
            var address = DeriveAddress(seed);
            _bitcoinProvider.Initialize(address);
        }

        /// <inheritdoc/>
        public string DeriveAddress(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(BitcoinChain.DerivationPath);
            return derived.PrivateKey.PubKey
                .GetAddress(ScriptPubKeyType.Legacy, BitcoinChain.CurrentNetwork)
                .ToString();
        }

        /// <inheritdoc/>
        public async Task<WalletBalanceResponse?> GetBalanceAsync(string address, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<WalletBalanceResponse>($"api/bitcoin/{address}/balance", cancellationToken);

            return ExtractData<WalletBalanceResponse>(result);
        }

        /// <inheritdoc/>
        public async Task<TransactionListResponse?> GetTransactionsAsync(string address, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<TransactionListResponse>($"api/bitcoin/{address}/transactions", cancellationToken);

            return ExtractData<TransactionListResponse>(result);
        }

        /// <inheritdoc/>
        public async Task<ExchangeRateResponse?> GetExchangeRateAsync(string toAssetSymbol, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<ExchangeRateResponse>($"api/crypto/exchange-rate/BTC/{toAssetSymbol}", cancellationToken);

            return ExtractData<ExchangeRateResponse>(result);
        }

        /// <inheritdoc/>
        public async Task<BroadcastTransactionResponse?> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .PostAsync<BroadcastTransactionRequest, BroadcastTransactionResponse>(
                    "api/bitcoin/broadcast", request, cancellationToken);

            return ExtractData<BroadcastTransactionResponse>(result);
        }

        /// <inheritdoc/>
        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_bitcoinProvider.Address))
            {
                return;
            }

            var balance = await GetBalanceAsync(_bitcoinProvider.Address, cancellationToken);
            var rate = await GetExchangeRateAsync("EUR", cancellationToken);
            var transactions = await GetTransactionsAsync(_bitcoinProvider.Address, cancellationToken);

            if (balance is null || rate is null || transactions is null)
            {
                return;
            }

            _bitcoinProvider.Set(
                balance.Balance,
                rate.Rate,
                transactions.Transactions.ToList().AsReadOnly(),
                transactions.Utxos.ToList().AsReadOnly());
        }

        /// <inheritdoc/>
        public Task SignAndBroadcastAsync(string toAddress, decimal amount, CancellationToken cancellationToken = default)
        {
            _pinEntrySlidingPanel.ShowPanel(async pin =>
                await ExecuteSignAndBroadcastAsync(pin, toAddress, amount, cancellationToken));

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public string SignTransaction(BitcoinSignRequest request)
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
                var signedHex = SignTransaction(new BitcoinSignRequest
                {
                    Seed = decryptedSeed,
                    ToAddress = toAddress,
                    AmountBtc = amount,
                    Utxos = _bitcoinProvider.Utxos,
                    FeeBtc = BitcoinFees.Default
                });

                await BroadcastAndNotifyAsync(signedHex, cancellationToken);
            }
            catch (FormatException)
            {
                MainWindow.Instance.ShowNotification(
                    "Invalid Address",
                    "Please enter a valid Bitcoin address.");
            }
            catch (NotEnoughFundsException)
            {
                MainWindow.Instance.ShowNotification(
                    "Insufficient Funds",
                    "Your balance is too low to cover this amount and fees.");
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification(
                    "Signing Error",
                    ex.Message);
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

        private static string BuildAndSignTransaction(byte[] privateKeyBytes, BitcoinSignRequest request)
        {
            var key = new Key(privateKeyBytes);
            var sourceAddress = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, BitcoinChain.CurrentNetwork);
            var destinationAddress = BitcoinAddress.Create(request.ToAddress, BitcoinChain.CurrentNetwork);

            var coins = request.Utxos.Select(utxo =>
                new Coin(
                    new OutPoint(uint256.Parse(utxo.TransactionId), (uint)utxo.OutputIndex),
                    new TxOut(Money.Coins(utxo.Amount), sourceAddress.ScriptPubKey)));

            var builder = BitcoinChain.CurrentNetwork.CreateTransactionBuilder()
                .AddCoins(coins)
                .AddKeys(key)
                .Send(destinationAddress, Money.Coins(request.AmountBtc))
                .SetChange(sourceAddress)
                .SendFees(Money.Coins(request.FeeBtc));

            var transaction = builder.BuildTransaction(sign: true);

            if (!builder.Verify(transaction, out var errors))
            {
                throw new InvalidOperationException(
                    $"Transaction verification failed: {string.Join(", ", errors.Select(e => e.ToString()))}");
            }

            return transaction.ToHex();
        }

        private static byte[] DerivePrivateKey(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(BitcoinChain.DerivationPath);
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
    }
}