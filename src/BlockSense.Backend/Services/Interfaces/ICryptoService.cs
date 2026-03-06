using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface ICryptoService
    {
        Task<WalletBalanceResponse> GetBalanceAsync(string address);
        Task<TransactionListResponse> GetTransactionsAsync(string address);
        Task<BroadcastTransactionResponse> BroadcastAsync(BroadcastTransactionRequest request);
    }
}
