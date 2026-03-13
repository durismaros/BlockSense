using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;

namespace BlockSense.Backend.Services.Interfaces
{
    /// <summary>
    /// Defines operations for interacting with a cryptocurrency network.
    /// </summary>
    public interface ICryptoService
    {
        /// <summary>
        /// Retrieves the current balance for the specified wallet address.
        /// </summary>
        /// <param name="address">The wallet address to query.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="WalletBalanceResponse"/> containing the address and its current balance.</returns>
        Task<WalletBalanceResponse> GetBalanceAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the next available nonce for the specified wallet address.
        /// </summary>
        /// <param name="address">The wallet address to query.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="NextNonceResponse"/> containing the next available nonce value.</returns>
        Task<NextNonceResponse> GetNextAvailableNonce(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the transaction history for the specified wallet address.
        /// </summary>
        /// <param name="address">The wallet address to query.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="TransactionListResponse"/> containing the transactions for the address.</returns>
        Task<TransactionListResponse> GetTransactionsAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Broadcasts a signed transaction to the network.
        /// </summary>
        /// <param name="request">The request containing the signed transaction hex.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="BroadcastTransactionResponse"/> containing the resulting transaction ID.</returns>
        Task<BroadcastTransactionResponse> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default);
    }
}