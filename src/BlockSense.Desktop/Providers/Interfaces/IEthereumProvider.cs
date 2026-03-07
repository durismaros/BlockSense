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

        decimal EurValue
        {
            get;
        }

        IReadOnlyList<TransactionDto> Transactions
        {
            get;
        }

        DateTime? LastRefreshed
        {
            get;
        }

        event Action? OnChanged;

        Task RefreshAsync(CancellationToken cancellationToken = default);
    }
}
