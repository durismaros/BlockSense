using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlockSense.Desktop;

public partial class WalletSelectionView : UserControl
{
    private readonly NavigationManager _navigationManager;

    public WalletSelectionView()
    {
        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        InitializeComponent();


        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private async void ToHomeViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<HomeView>();
    }

    private async void ToRecoveryPhraseViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<RecoveryPhraseView>();
    }

    private async void ToRecoveryPhraseImportViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<RecoveryPhraseImportView>();
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        HomeButton.Click += ToHomeViewClick;
        CreateWalletButton.Click += ToRecoveryPhraseViewClick;
        ImportWalletButton.Click += ToRecoveryPhraseImportViewClick;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        HomeButton.Click -= ToHomeViewClick;
        CreateWalletButton.Click -= ToRecoveryPhraseViewClick;
        ImportWalletButton.Click -= ToRecoveryPhraseImportViewClick;
    }
}