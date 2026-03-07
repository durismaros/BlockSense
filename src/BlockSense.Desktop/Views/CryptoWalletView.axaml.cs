using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.Enums;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class CryptoWalletView : UserControl
{
    private enum ActiveTab { All, Bitcoin, Ethereum }

    private readonly IBitcoinProvider _bitcoinProvider;
    private readonly IEthereumProvider _ethereumProvider;

    private ActiveTab _activeTab = ActiveTab.All;
    private CancellationTokenSource? _cancellationTokenSource;

    private TextBlock? _allTabLabel;
    private TextBlock? _btcTabLabel;
    private TextBlock? _ethTabLabel;

    public CryptoWalletView()
    {
        _bitcoinProvider = App.ServiceProvider.GetRequiredService<IBitcoinProvider>()
            ?? throw new ArgumentNullException(nameof(IBitcoinProvider));

        _ethereumProvider = App.ServiceProvider.GetRequiredService<IEthereumProvider>()
            ?? throw new ArgumentNullException(nameof(IEthereumProvider));

        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void CacheTabLabels()
    {
        _allTabLabel = AllTabButton.Content as TextBlock;
        _btcTabLabel = BtcTabButton.Content as TextBlock;
        _ethTabLabel = EthTabButton.Content as TextBlock;
    }

    private void WireButtons()
    {
        BackButton.Click += (_, _) => NavigateBack();

        CopyBtcAddressButton.Click += (_, _) => CopyToClipboard(BtcAddressText.Text);
        CopyEthAddressButton.Click += (_, _) => CopyToClipboard(EthAddressText.Text);

        // Per-card refresh buttons
        RefreshBtcButton.Click += async (_, _) =>
        {
            RefreshBtcButton.IsEnabled = false;
            await _bitcoinProvider.RefreshAsync(_cancellationTokenSource?.Token ?? default);
            RefreshBtcButton.IsEnabled = true;
        };

        RefreshEthButton.Click += async (_, _) =>
        {
            RefreshEthButton.IsEnabled = false;
            await _ethereumProvider.RefreshAsync(_cancellationTokenSource?.Token ?? default);
            RefreshEthButton.IsEnabled = true;
        };

        SendButton.Click += (_, _) => NavigateToSend();
        ReceiveButton.Click += (_, _) => NavigateToReceive();

        AllTabButton.Click += (_, _) => SetTab(ActiveTab.All);
        BtcTabButton.Click += (_, _) => SetTab(ActiveTab.Bitcoin);
        EthTabButton.Click += (_, _) => SetTab(ActiveTab.Ethereum);
    }

    private void RenderAddresses()
    {
        BtcAddressText.Text = _bitcoinProvider.Address;
        EthAddressText.Text = _ethereumProvider.Address;
    }
    private void OnBitcoinChanged()
        => Dispatcher.UIThread.Post(UpdateBitcoinUi);

    private void OnEthereumChanged()
        => Dispatcher.UIThread.Post(UpdateEthereumUi);

    private void UpdateBitcoinUi()
    {
        BtcUsdValueText.Text = _bitcoinProvider.EurValue > 0
            ? $"≈ ${_bitcoinProvider.EurValue:N2} EUR"
            : string.Empty;

        UpdateLastRefreshed();
        RenderTransactions();
    }

    private void UpdateEthereumUi()
    {
        EthUsdValueText.Text = _ethereumProvider.EurValue > 0
            ? $"≈ ${_ethereumProvider.EurValue:N2} EUR"
            : string.Empty;

        UpdateLastRefreshed();
        RenderTransactions();
    }

    private void UpdateLastRefreshed()
    {
        var btcTime = _bitcoinProvider.LastRefreshed;
        var ethTime = _ethereumProvider.LastRefreshed;
        var latest = btcTime > ethTime ? btcTime : ethTime;

        LastRefreshedText.Text = latest.HasValue
            ? $"Updated {latest.Value.ToLocalTime():HH:mm}"
            : string.Empty;
    }

    private void RenderTransactions()
    {
        var transactions = _activeTab switch
        {
            ActiveTab.Bitcoin => _bitcoinProvider.Transactions,
            ActiveTab.Ethereum => _ethereumProvider.Transactions,
            _ => _bitcoinProvider.Transactions
                    .Concat(_ethereumProvider.Transactions)
                    .OrderByDescending(t => t.Timestamp)
                    .ToList()
        };

        TransactionsPanel.Children.Clear();

        if (!transactions.Any())
        {
            TransactionsPanel.Children.Add(EmptyStatePanel);
            return;
        }

        var isFirst = true;
        foreach (var tx in transactions)
        {
            if (!isFirst)
                TransactionsPanel.Children.Add(BuildDivider());

            TransactionsPanel.Children.Add(BuildTransactionRow(tx));
            isFirst = false;
        }
    }

    private static Border BuildDivider()
        => new() { Height = 1, Background = new SolidColorBrush(Color.Parse("#1E1E21")), Margin = new Thickness(0) };

    private static Border BuildTransactionRow(TransactionDto tx)
    {
        var isIncoming = tx.Amount >= 0;

        var amountColor = tx.Status == TransactionStatus.Failed
            ? Color.Parse("#4A4845")
            : isIncoming
                ? Color.Parse("#3D9970")
                : Color.Parse("#C0392B");

        var statusColor = tx.Status switch
        {
            TransactionStatus.Confirmed => Color.Parse("#3D9970"),
            TransactionStatus.Pending => Color.Parse("#C8A837"),
            TransactionStatus.Failed => Color.Parse("#C0392B"),
            _ => Color.Parse("#4A4845")
        };

        var statusText = tx.Status switch
        {
            TransactionStatus.Confirmed => "●  Confirmed",
            TransactionStatus.Pending => "●  Pending",
            TransactionStatus.Failed => "●  Failed",
            _ => "●  Unknown"
        };

        var shortHash = tx.TxHash.Length > 16
            ? $"{tx.TxHash[..8]}···{tx.TxHash[^8..]}"
            : tx.TxHash;

        var amountPrefix = isIncoming ? "+" : "−";
        var directionLabel = isIncoming
            ? $"From  {Shorten(tx.FromAddress)}"
            : $"To      {Shorten(tx.ToAddress)}";

        return new Border
        {
            Classes = { "TxRow" },
            Padding = new Thickness(20, 14),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto * Auto"),
                Children =
                {
                    // Direction dot
                    PlaceInColumn(0, new Border
                    {
                        Width = 8,
                        Height = 8,
                        CornerRadius = new CornerRadius(4),
                        Background = new SolidColorBrush(isIncoming
                            ? Color.Parse("#3D9970")
                            : Color.Parse("#C0392B")),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 14, 0)
                    }),

                    // Left — hash + address
                    PlaceInColumn(1, new StackPanel
                    {
                        Spacing = 3,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Children =
                        {
                            Txt(shortHash, "#A0A09A", 12, FontWeight.Medium, "Consolas"),
                            Txt(directionLabel, "#4A4845", 11),
                            Txt(tx.Timestamp.ToString("dd MMM yyyy · HH:mm"), "#2E2E32", 10)
                        }
                    }),

                    // Right — amount + status
                    PlaceInColumn(2, new StackPanel
                    {
                        Spacing = 4,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Children =
                        {
                            Txt($"{amountPrefix}{Math.Abs(tx.Amount):F6} {tx.Currency}",
                                amountColor, 13, FontWeight.SemiBold,
                                align: Avalonia.Layout.HorizontalAlignment.Right),
                            Txt(statusText, statusColor, 10, FontWeight.Medium,
                                align: Avalonia.Layout.HorizontalAlignment.Right)
                        }
                    })
                }
            }
        };
    }

    private void SetTab(ActiveTab tab)
    {
        _activeTab = tab;

        AllTabButton.Classes.Set("Active", tab == ActiveTab.All);
        BtcTabButton.Classes.Set("Active", tab == ActiveTab.Bitcoin);
        EthTabButton.Classes.Set("Active", tab == ActiveTab.Ethereum);

        if (_allTabLabel is not null)
            _allTabLabel.Foreground = new SolidColorBrush(Color.Parse(
                tab == ActiveTab.All ? "#F0EDE8" : "#7A7870"));

        if (_btcTabLabel is not null)
            _btcTabLabel.Foreground = new SolidColorBrush(Color.Parse(
                tab == ActiveTab.Bitcoin ? "#F0EDE8" : "#7A7870"));

        if (_ethTabLabel is not null)
            _ethTabLabel.Foreground = new SolidColorBrush(Color.Parse(
                tab == ActiveTab.Ethereum ? "#F0EDE8" : "#7A7870"));

        RenderTransactions();
    }

    private void NavigateBack()
    {
        // Wire up your navigation service here
    }

    private void NavigateToSend()
    {
        // Wire up your navigation service here
    }

    private void NavigateToReceive()
    {
        // Wire up your navigation service here
    }

    private async Task RefreshAllAsync()
    {
        var token = _cancellationTokenSource?.Token ?? default;
        await Task.WhenAll(
            _bitcoinProvider.RefreshAsync(token),
            _ethereumProvider.RefreshAsync(token));
    }

    private void CopyToClipboard(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        _ = TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(text);
    }

    private static Control PlaceInColumn(int column, Control child)
    {
        Grid.SetColumn(child, column);
        return child;
    }

    private static TextBlock Txt(
        string text,
        Color color,
        double size,
        FontWeight weight = FontWeight.Normal,
        string? fontFamily = null,
        Avalonia.Layout.HorizontalAlignment align = Avalonia.Layout.HorizontalAlignment.Left)
        => new()
        {
            Text = text,
            Foreground = new SolidColorBrush(color),
            FontSize = size,
            FontWeight = weight,
            FontFamily = fontFamily is not null ? new FontFamily(fontFamily) : FontFamily.Default,
            HorizontalAlignment = align
        };

    private static TextBlock Txt(
        string text,
        string hex,
        double size,
        FontWeight weight = FontWeight.Normal,
        string? fontFamily = null,
        Avalonia.Layout.HorizontalAlignment align = Avalonia.Layout.HorizontalAlignment.Left)
        => Txt(text, Color.Parse(hex), size, weight, fontFamily, align);

    private static string Shorten(string address)
        => address.Length > 20
            ? $"{address[..6]}···{address[^6..]}"
            : address;

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _cancellationTokenSource = new CancellationTokenSource();

        CacheTabLabels();
        WireButtons();
        RenderAddresses();

        _bitcoinProvider.OnChanged += OnBitcoinChanged;
        _ethereumProvider.OnChanged += OnEthereumChanged;

        _ = RefreshAllAsync();
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _bitcoinProvider.OnChanged -= OnBitcoinChanged;
        _ethereumProvider.OnChanged -= OnEthereumChanged;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}