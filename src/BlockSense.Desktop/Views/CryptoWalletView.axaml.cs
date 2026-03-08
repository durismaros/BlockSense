using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.Enums;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class CryptoWalletView : UserControl
{
    private enum ActiveTab { All, Bitcoin, Ethereum }
    private enum SendCurrency { Bitcoin, Ethereum }

    private readonly IBitcoinProvider _bitcoinProvider;
    private readonly IEthereumProvider _ethereumProvider;
    private readonly NavigationManager _navigationManager;
    private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;

    private ActiveTab _activeTab = ActiveTab.All;
    private SendCurrency _sendCurrency = SendCurrency.Bitcoin;
    private CancellationTokenSource? _cancellationTokenSource;

    public CryptoWalletView()
    {
        _bitcoinProvider = App.ServiceProvider.GetRequiredService<IBitcoinProvider>();
        _ethereumProvider = App.ServiceProvider.GetRequiredService<IEthereumProvider>();
        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>();
        _pinEntrySlidingPanel = MainWindow.Instance.PinEntrySlidingPanel;

        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _cancellationTokenSource = new CancellationTokenSource();

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

    private void WireButtons()
    {
        BackButton.Click += (_, _) => NavigateBack();
        RemoveWalletButton.Click += async (_, _) => await ShowConfirmDeleteAsync();
        ConfirmDeleteButton.Click += async (_, _) => await DeleteWalletAsync();

        SendButton.Click += async (_, _) => await OpenSendOverlayAsync();
        CloseSendOverlayButton.Click += async (_, _) => await CloseSendOverlayAsync();
        ConfirmSendButton.Click += (_, _) => ExecuteSend();

        SendBtcToggle.Click += (_, _) => SetSendCurrency(SendCurrency.Bitcoin);
        SendEthToggle.Click += (_, _) => SetSendCurrency(SendCurrency.Ethereum);

        SendAmountInput.TextChanged += (_, _) => UpdateConversion();

        CopyBtcAddressButton.Click += (_, _) => CopyToClipboard(BtcAddressText.Text);
        CopyEthAddressButton.Click += (_, _) => CopyToClipboard(EthAddressText.Text);

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

        AllTabButton.Click += (_, _) => SetTab(ActiveTab.All);
        BtcTabButton.Click += (_, _) => SetTab(ActiveTab.Bitcoin);
        EthTabButton.Click += (_, _) => SetTab(ActiveTab.Ethereum);
    }

    // ── Confirm Delete ────────────────────────────────────────────────────────

    private async Task ShowConfirmDeleteAsync()
    {
        ConfirmDeleteButton.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(ConfirmDeleteButton);
        await Task.Delay(3000);
        await Animations.FadeOutAnimation.RunAsync(ConfirmDeleteButton);
        ConfirmDeleteButton.IsVisible = false;
    }

    private async Task DeleteWalletAsync()
    {
        // Dismiss the confirm button immediately
        ConfirmDeleteButton.IsVisible = false;

        // TODO: call provider to remove wallet

        await _navigationManager.NavigateToAsync<HomeView>();
    }

    // ── Send Overlay ──────────────────────────────────────────────────────────

    private async Task OpenSendOverlayAsync()
    {
        // Reset state
        SendAddressInput.Text = string.Empty;
        SendAmountInput.Text = string.Empty;
        SetSendCurrency(SendCurrency.Bitcoin);
        UpdateConversion();

        SendOverlay.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(SendOverlay);
    }

    private async Task CloseSendOverlayAsync()
    {
        await Animations.FadeOutAnimation.RunAsync(SendOverlay);
        SendOverlay.IsVisible = false;
    }

    private void SetSendCurrency(SendCurrency currency)
    {
        _sendCurrency = currency;

        SendBtcToggle.Classes.Set("Active", currency == SendCurrency.Bitcoin);
        SendEthToggle.Classes.Set("Active", currency == SendCurrency.Ethereum);

        // Update toggle text colours to contrast against active/inactive backgrounds
        SendBtcToggleText.Foreground = new SolidColorBrush(Color.Parse(
            currency == SendCurrency.Bitcoin ? "#E8DDD0" : "#4A3F30"));
        SendEthToggleText.Foreground = new SolidColorBrush(Color.Parse(
            currency == SendCurrency.Ethereum ? "#E8DDD0" : "#4A3F30"));

        UpdateConversion();
    }

    private void UpdateConversion()
    {
        var amountStr = SendAmountInput?.Text ?? string.Empty;
        if (!decimal.TryParse(amountStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount))
            amount = 0;

        decimal rate;
        string ticker;

        if (_sendCurrency == SendCurrency.Bitcoin)
        {
            rate = _bitcoinProvider.ExchangeRate;
            ticker = "BTC";
        }
        else
        {
            rate = _ethereumProvider.ExchangeRate;
            ticker = "ETH";
        }

        var eurValue = amount * rate;

        if (SendConversionText is not null)
            SendConversionText.Text = $"€{eurValue:N2}";

        if (SendConversionLabel is not null)
            SendConversionLabel.Text = $"≈ EUR value for {amount:F6} {ticker}";

        if (SendExchangeRateText is not null)
            SendExchangeRateText.Text = $"1 {ticker} = €{rate:N2}";
    }

    private void ExecuteSend()
    {
        var address = SendAddressInput.Text ?? string.Empty;
        var amountStr = SendAmountInput.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(address) || !double.TryParse(amountStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount))
            return;

        _pinEntrySlidingPanel.ShowPanel(async confirmedPin =>
        {

        });
    }

    // ── Providers ─────────────────────────────────────────────────────────────

    private void OnBitcoinChanged() => Dispatcher.UIThread.Post(UpdateBitcoinUi);
    private void OnEthereumChanged() => Dispatcher.UIThread.Post(UpdateEthereumUi);

    private void UpdateBitcoinUi()
    {
        BtcBalanceText.Text = $"{_bitcoinProvider.Balance:F8} BTC";
        BtcEurValueText.Text = $"≈ €{_bitcoinProvider.Balance * _bitcoinProvider.ExchangeRate:N2} EUR";
        RenderTransactions();
        UpdateConversion();
    }

    private void UpdateEthereumUi()
    {
        EthBalanceText.Text = $"{_ethereumProvider.Balance:F8} ETH";
        EthEurValueText.Text = $"≈ €{_ethereumProvider.Balance * _ethereumProvider.ExchangeRate:N2} EUR";
        RenderTransactions();
        UpdateConversion();
    }

    private void RenderAddresses()
    {
        BtcAddressText.Text = _bitcoinProvider.Address;
        EthAddressText.Text = _ethereumProvider.Address;
    }

    // ── Tabs ──────────────────────────────────────────────────────────────────

    private void SetTab(ActiveTab tab)
    {
        _activeTab = tab;

        AllTabButton.Classes.Set("Active", tab == ActiveTab.All);
        BtcTabButton.Classes.Set("Active", tab == ActiveTab.Bitcoin);
        EthTabButton.Classes.Set("Active", tab == ActiveTab.Ethereum);

        RenderTransactions();
    }

    // ── Transactions ──────────────────────────────────────────────────────────

    private void RenderTransactions()
    {
        var transactions = GetFilteredTransactions();

        TransactionsPanel.Children.Clear();

        if (!transactions.Any())
        {
            TransactionsPanel.Children.Add(BuildEmptyState());
            return;
        }

        for (var i = 0; i < transactions.Count; i++)
        {
            if (i > 0)
                TransactionsPanel.Children.Add(BuildDivider());

            TransactionsPanel.Children.Add(BuildTransactionRow(transactions[i]));
        }
    }

    private List<TransactionDto> GetFilteredTransactions() => _activeTab switch
    {
        ActiveTab.Bitcoin => _bitcoinProvider.Transactions.ToList(),
        ActiveTab.Ethereum => _ethereumProvider.Transactions.ToList(),
        _ => _bitcoinProvider.Transactions
                .Concat(_ethereumProvider.Transactions)
                .OrderByDescending(t => t.Timestamp)
                .ToList()
    };

    private static Control BuildEmptyState() => new Border
    {
        Padding = new Thickness(40),
        Child = new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Children =
            {
                new Viewbox
                {
                    Width = 48, Height = 48,
                    Child = new Path
                    {
                        Data = Avalonia.Media.Geometry.Parse(
                            "M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2ZM13 17H11V15H13V17ZM13 13H11V7H13V13Z"),
                        Fill = new SolidColorBrush(Color.Parse("#9A8F7E")),
                        Stretch = Stretch.Uniform
                    }
                },
                Txt("No transactions yet", "#6B5E4D", 15, FontWeight.SemiBold,
                    align: Avalonia.Layout.HorizontalAlignment.Center),
                Txt("Your transaction history will appear here", "#6B5E4D", 12,
                    align: Avalonia.Layout.HorizontalAlignment.Center)
            }
        }
    };

    private static Border BuildDivider() => new()
    {
        Height = 1,
        Background = new SolidColorBrush(Color.Parse("#C0B8A8")),
        Margin = new Thickness(20, 0)
    };

    private static Border BuildTransactionRow(TransactionDto tx)
    {
        var isIncoming = tx.Amount >= 0;

        var amountColor = tx.Status == TransactionStatus.Failed
            ? "#9A8F7E"
            : isIncoming ? "#3D9970" : "#C0392B";

        var (statusColor, statusText) = tx.Status switch
        {
            TransactionStatus.Confirmed => ("#3D9970", "● Confirmed"),
            TransactionStatus.Pending => ("#C8A837", "● Pending"),
            TransactionStatus.Failed => ("#C0392B", "● Failed"),
            _ => ("#9A8F7E", "● Unknown")
        };

        var directionLabel = isIncoming
            ? $"From  {tx.FromAddress}"
            : $"To      {tx.ToAddress}";

        var amountText = $"{(isIncoming ? "+" : "−")}{Math.Abs(tx.Amount):F6} {tx.Currency}";

        var row = new Border { Classes = { "TxRow" } };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto * Auto") };

        var dot = new Border
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(Color.Parse(isIncoming ? "#3D9970" : "#C0392B")),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 14, 0)
        };
        Grid.SetColumn(dot, 0);

        var left = new StackPanel
        {
            Spacing = 3,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Children =
            {
                Txt(tx.TxHash, "#6B5E4D", 12, FontWeight.Medium, "Consolas"),
                Txt(directionLabel, "#4A4845", 11),
                Txt(tx.Timestamp.ToString("dd MMM yyyy · HH:mm"), "#9A8F7E", 10)
            }
        };
        Grid.SetColumn(left, 1);

        var right = new StackPanel
        {
            Spacing = 4,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Children =
            {
                Txt(amountText, amountColor, 13, FontWeight.SemiBold,
                    align: Avalonia.Layout.HorizontalAlignment.Right),
                Txt(statusText, statusColor, 10, FontWeight.Medium,
                    align: Avalonia.Layout.HorizontalAlignment.Right)
            }
        };
        Grid.SetColumn(right, 2);

        grid.Children.Add(dot);
        grid.Children.Add(left);
        grid.Children.Add(right);
        row.Child = grid;

        return row;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private async void NavigateBack()
    {
        await _navigationManager.NavigateToAsync<HomeView>();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task RefreshAllAsync()
    {
        var token = _cancellationTokenSource?.Token ?? default;
        await Task.WhenAll(
            _bitcoinProvider.RefreshAsync(token),
            _ethereumProvider.RefreshAsync(token));
    }

    private void CopyToClipboard(string? text)
    {
        if (!string.IsNullOrWhiteSpace(text))
            _ = TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(text);
    }

    private static string Shorten(string address) =>
        address.Length > 20 ? $"{address[..6]}···{address[^6..]}" : address;

    private static TextBlock Txt(
        string text,
        string hex,
        double size,
        FontWeight weight = FontWeight.Normal,
        string? fontFamily = null,
        Avalonia.Layout.HorizontalAlignment align = Avalonia.Layout.HorizontalAlignment.Left) => new()
        {
            Text = text,
            Foreground = new SolidColorBrush(Color.Parse(hex)),
            FontSize = size,
            FontWeight = weight,
            FontFamily = fontFamily is not null ? new FontFamily(fontFamily) : FontFamily.Default,
            HorizontalAlignment = align
        };
}