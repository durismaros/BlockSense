using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Wallet;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for interacting with the Bitcoin blockchain,
    /// including balance retrieval, transaction management, and transaction signing.
    /// </summary>
    public interface IBitcoinService
    {
        /// <summary>
        /// Initializes the Bitcoin wallet by deriving the address from the provided seed
        /// and registering it with the provider.
        /// </summary>
        /// <param name="seed">The raw seed bytes used to derive the Bitcoin address.</param>
        void Initialize(byte[] seed);

        /// <summary>
        /// Derives a Bitcoin address from the provided seed using the configured derivation path.
        /// </summary>
        /// <param name="seed">The raw seed bytes used for key derivation.</param>
        /// <returns>The derived Bitcoin address as a string.</returns>
        string DeriveAddress(byte[] seed);

        /// <summary>
        /// Retrieves the Bitcoin balance for the specified address.
        /// </summary>
        /// <param name="address">The Bitcoin address to query.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="WalletBalanceResponse"/> containing the balance,
        /// or <c>null</c> if the request failed.
        /// </returns>
        Task<WalletBalanceResponse?> GetBalanceAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the transaction history for the specified Bitcoin address.
        /// </summary>
        /// <param name="address">The Bitcoin address to query.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="TransactionListResponse"/> containing transactions and UTXOs,
        /// or <c>null</c> if the request failed.
        /// </returns>
        Task<TransactionListResponse?> GetTransactionsAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the current exchange rate from BTC to the specified target currency.
        /// </summary>
        /// <param name="toAssetSymbol">The target asset or currency symbol (e.g., "EUR", "USD").</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="ExchangeRateResponse"/> containing the rate,
        /// or <c>null</c> if the request failed.
        /// </returns>
        Task<ExchangeRateResponse?> GetExchangeRateAsync(string toAssetSymbol, CancellationToken cancellationToken = default);

        /// <summary>
        /// Broadcasts a signed Bitcoin transaction to the network.
        /// </summary>
        /// <param name="request">The request containing the signed transaction hex.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="BroadcastTransactionResponse"/> with the transaction ID,
        /// or <c>null</c> if the broadcast failed.
        /// </returns>
        Task<BroadcastTransactionResponse?> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the current wallet state by fetching the latest balance, exchange rate, and transactions.
        /// Does nothing if no Bitcoin address is currently loaded.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task RefreshAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Prompts the user for their PIN, signs the transaction using the decrypted seed,
        /// and broadcasts it to the Bitcoin network.
        /// </summary>
        /// <param name="toAddress">The destination Bitcoin address.</param>
        /// <param name="amount">The amount of BTC to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task SignAndBroadcastAsync(string toAddress, decimal amount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs a Bitcoin transaction using the private key derived from the provided seed.
        /// </summary>
        /// <param name="request">The sign request containing the seed, destination, amount, UTXOs, and fees.</param>
        /// <returns>The signed transaction as a hex-encoded string.</returns>
        string SignTransaction(BitcoinSignRequest request);
    }
}