using Avalonia;
using Avalonia.Controls;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlockSense.Desktop;

public partial class CryptoWalletView : UserControl
{
    private readonly IWalletProvider _walletProvider;
    private readonly IWalletService _walletService;

    public CryptoWalletView()
    {
        _walletProvider = App.ServiceProvider.GetRequiredService<IWalletProvider>()
            ?? throw new ArgumentNullException(nameof(IWalletProvider));

        _walletService = App.ServiceProvider.GetRequiredService<IWalletService>()
            ?? throw new ArgumentNullException(nameof(IWalletService));

        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        BtcAddressText.Text = _walletProvider.Wallet!.BtcAddress;
        EthAddressText.Text = _walletProvider.Wallet!.EthAddress;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {

    }
}