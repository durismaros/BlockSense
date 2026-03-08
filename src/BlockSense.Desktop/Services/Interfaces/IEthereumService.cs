using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Wallet;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IEthereumService
    {
        Task<WalletBalanceResponse?> GetBalanceAsync(string address, CancellationToken cancellationToken = default);
        Task<TransactionListResponse?> GetTransactionsAsync(string address, CancellationToken cancellationToken = default);
        Task<ExchangeRateResponse?> GetExchangeRateAsync(string toAssetSymbol, CancellationToken cancellationToken = default);
        Task<BroadcastTransactionResponse?> BroadcastAsync(BroadcastTransactionRequest request, CancellationToken cancellationToken = default);
        string DeriveAddress(byte[] seed);
        string SignTransaction(EthereumSignRequest request);
    }
}
