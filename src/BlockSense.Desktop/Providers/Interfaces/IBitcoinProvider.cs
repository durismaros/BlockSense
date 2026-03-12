using BlockSense.Contracts.DTOs.Transaction;
using System;
using System.Collections.Generic;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IBitcoinProvider
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

        void Initialize(string address);
        void Set(decimal balance, decimal exchangeRate, IReadOnlyList<TransactionDto> transactions);
        void Clear();
    }
}
