using BlockSense.Contracts.DTOs.Transaction;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IEthereumProvider
    {
        string Address
        {
            get;
        }

        decimal Balance
        {
            get;
        }

        decimal ExchangeRate
        {
            get;
        }

        IReadOnlyList<TransactionDto> Transactions
        {
            get;
        }

        event Action? OnChanged;

        void Initialize(byte[] seed);
        Task RefreshAsync(CancellationToken cancellationToken = default);
    }
}
