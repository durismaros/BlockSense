using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class BitcoinService : IBitcoinService
    {
        private readonly IApiClient _apiClient;
        private readonly ICurrentWalletProvider _currentWalletProvider;
        private readonly IBitcoinProvider _bitcoinProvider;
        private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;

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

        public async Task<ExchangeRateResponse?> GetExchangeRateAsync(string toAssetSymbol, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<ExchangeRateResponse>($"/api/crypto/exchange-rate/BTC/{toAssetSymbol}", cancellationToken);

            if (result.IsSuccess && result is ApiResult<ExchangeRateResponse>.Success success)
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

        public void Initialize(byte[] seed)
        {
            var address = DeriveAddress(seed);
            _bitcoinProvider.Initialize(address);
        }

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_bitcoinProvider.Address))
            {
                return;
            }

            var balance = await GetBalanceAsync(_bitcoinProvider.Address, cancellationToken);
            var rate = await GetExchangeRateAsync("EUR", cancellationToken);
            var txs = await GetTransactionsAsync(_bitcoinProvider.Address, cancellationToken);

            if (balance is null || rate is null || txs is null)
            {
                return;
            }

            _bitcoinProvider.Set(
                balance.Balance,
                rate.Rate,
                txs.Transactions.ToList().AsReadOnly(),
                txs.Utxos.ToList().AsReadOnly());
        }

        public async Task SignAndBroadcastAsync(
            string toAddress,
            decimal amount,
            CancellationToken cancellationToken = default)
        {
            _pinEntrySlidingPanel.ShowPanel(async pin =>
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

                    var result = await BroadcastAsync(
                        new BroadcastTransactionRequest { SignedTransactionHex = signedHex },
                        cancellationToken);

                    MainWindow.Instance.ShowNotification(
                        "Transaction Broadcast",
                        $"Transaction {result?.TransactionId} broadcasted");
                }
                catch (FormatException)
                {
                    MainWindow.Instance.ShowNotification(
                        "Invalid Address",
                        "Please enter a valid address.");
                }
                catch (NotEnoughFundsException)
                {
                    MainWindow.Instance.ShowNotification(
                        "Not Enough Funds",
                        "");
                }
                catch (Exception ex)
                {
                    MainWindow.Instance.ShowNotification("Error", ex.Message);
                }
                finally
                {
                    _pinEntrySlidingPanel.HidePanel();

                    CryptographicOperations.ZeroMemory(decryptedSeed);
                }
            });
        }

        public string DeriveAddress(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(BitcoinChain.DerivationPath);
            return derived.PrivateKey.PubKey
                .GetAddress(ScriptPubKeyType.Legacy, BitcoinChain.CurrentNetwork)
                .ToString();
        }

        private static byte[] DerivePrivateKey(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(BitcoinChain.DerivationPath);
            return derived.PrivateKey.ToBytes();
        }

        public string SignTransaction(BitcoinSignRequest request)
        {
            var privateKeyBytes = DerivePrivateKey(request.Seed);

            try
            {
                var key = new Key(privateKeyBytes);

                var source = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, BitcoinChain.CurrentNetwork);
                var destination = BitcoinAddress.Create(request.ToAddress, BitcoinChain.CurrentNetwork);

                var coins = BuildCoins(request, source);

                var tx = BitcoinChain.CurrentNetwork.CreateTransactionBuilder()
                    .AddCoins(coins)
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

        private static IEnumerable<Coin> BuildCoins(BitcoinSignRequest request, BitcoinAddress source)
        {
            return request.Utxos.SelectMany(u =>
            {
                var txHash = uint256.Parse(u.TransactionId);
                var amount = Money.Coins(u.Amount);
                var script = source.ScriptPubKey;

                // Try both vout 0 and vout 1 since we don't know the real index
                return new[]
                {
                    new Coin(txHash, 0, amount, script),
                    new Coin(txHash, 1, amount, script)
                };
            });
        }
    }
}
