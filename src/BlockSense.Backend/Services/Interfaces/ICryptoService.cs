using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface ICryptoService
    {
        Task<WalletBalanceResponse> GetBalanceAsync(string address, CancellationToken cancellationToken = default);
        Task<NextNonceResponse> GetNextAvailableNonce(string address, CancellationToken cancellationToken = default);
        Task<TransactionListResponse> GetTransactionsAsync(string address, CancellationToken cancellationToken = default);
        Task<BroadcastTransactionResponse> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default);
    }
}
