using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Desktop.Providers.Interfaces;
using System;
using System.Collections.Generic;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class BitcoinProvider : IBitcoinProvider
    {
        private Action? _onChanged;

        public string Address
        {
            get;
            private set;
        }

        public decimal Balance
        {
            get;
            private set;
        }

        public decimal ExchangeRate
        {
            get;
            private set;
        }

        public IReadOnlyList<TransactionDto> Transactions
        {
            get;
            private set; 
        }

        public event Action? OnChanged
        {
            add
            {
                _onChanged += value;
            }
            remove
            {
                _onChanged -= value;
            }
        }

        public BitcoinProvider()
        {
            Address = string.Empty;
            Balance = 0m;
            ExchangeRate = 0m;
            Transactions = Array.Empty<TransactionDto>();
        }

        public void Initialize(string address)
        {
            this.Address = address;
            _onChanged?.Invoke();
        }

        public void Set(decimal balance, decimal exchangeRate, IReadOnlyList<TransactionDto> transactions)
        {
            Balance = balance;
            ExchangeRate = exchangeRate;
            Transactions = transactions;
            _onChanged?.Invoke();
        }

        public void Clear()
        {
            Address = string.Empty;
            Balance = 0m;
            ExchangeRate = 0m;
            Transactions = Array.Empty<TransactionDto>();
        }
    }
}
