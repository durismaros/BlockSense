using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Wallet;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for interacting with the Ethereum blockchain,
    /// including balance retrieval, transaction management, and transaction signing.
    /// </summary>
    public interface IEthereumService
    {
        /// <summary>
        /// Initializes the Ethereum wallet by deriving the address from the provided seed
        /// and registering it with the provider.
        /// </summary>
        /// <param name="seed">The raw seed bytes used to derive the Ethereum address.</param>
        void Initialize(byte[] seed);

        /// <summary>
        /// Derives an Ethereum address from the provided seed using the configured derivation path.
        /// </summary>
        /// <param name="seed">The raw seed bytes used for key derivation.</param>
        /// <returns>The derived Ethereum address as a string.</returns>
        string DeriveAddress(byte[] seed);

        /// <summary>
        /// Retrieves the Ethereum balance for the specified address.
        /// </summary>
        /// <param name="address">The Ethereum address to query.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="WalletBalanceResponse"/> containing the balance,
        /// or <c>null</c> if the request failed.
        /// </returns>
        Task<WalletBalanceResponse?> GetBalanceAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the transaction history for the specified Ethereum address.
        /// </summary>
        /// <param name="address">The Ethereum address to query.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="TransactionListResponse"/> containing the transaction list,
        /// or <c>null</c> if the request failed.
        /// </returns>
        Task<TransactionListResponse?> GetTransactionsAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the current exchange rate from ETH to the specified target currency.
        /// </summary>
        /// <param name="toAssetSymbol">The target asset or currency symbol (e.g., "EUR", "USD").</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="ExchangeRateResponse"/> containing the rate,
        /// or <c>null</c> if the request failed.
        /// </returns>
        Task<ExchangeRateResponse?> GetExchangeRateAsync(string toAssetSymbol, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the next available nonce for the specified Ethereum address.
        /// </summary>
        /// <param name="address">The Ethereum address to query.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// The next available nonce as a <see cref="long"/>,
        /// or <c>null</c> if the request failed.
        /// </returns>
        Task<long?> GetNextAvailableNonceAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Broadcasts a signed Ethereum transaction to the network.
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
        /// Does nothing if no Ethereum address is currently loaded.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task RefreshAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Prompts the user for their PIN, signs the transaction using the decrypted seed,
        /// and broadcasts it to the Ethereum network.
        /// </summary>
        /// <param name="toAddress">The destination Ethereum address.</param>
        /// <param name="amount">The amount of ETH to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task SignAndBroadcastAsync(string toAddress, decimal amount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs an Ethereum transaction using the private key derived from the provided seed.
        /// </summary>
        /// <param name="request">The sign request containing the seed, destination, amount, nonce, gas settings, and chain ID.</param>
        /// <returns>The signed transaction as a hex-encoded string.</returns>
        string SignTransaction(EthereumSignRequest request);
    }
}