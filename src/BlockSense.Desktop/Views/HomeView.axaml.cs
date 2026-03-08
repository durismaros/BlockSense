using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlockSense.Desktop;

public partial class HomeView : UserControl
{
    private readonly IWalletService _walletService;
    private readonly NavigationManager _navigationManager;

    public HomeView()
    {
        _walletService = App.ServiceProvider.GetRequiredService<IWalletService>()
            ?? throw new ArgumentNullException(nameof(IWalletService));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private async void OnDashboardButtonClicked(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<UserDashboardView>();
    }

    private async void OnWalletButtonClicked(object? sender, RoutedEventArgs e)
    {
        var walletExists = await _walletService.WalletExistsAsync();

        if (walletExists)
        {
            await _walletService.UnlockWalletAsync();
        }
        else
        {
            await _navigationManager.NavigateToAsync<WalletSelectionView>();
        }
    }

    private async void FadeInText()
    {
        // Create and run fade-in animation
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(3),
            Easing = new SineEaseInOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(TextBlock.OpacityProperty, 1.0) }
                }
            }
        };

        await animation.RunAsync(WelcomeText);
        WelcomeText.Opacity = 1.0; // Set final opacity
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UserDashboardButton.Click += OnDashboardButtonClicked;
        UserWalletButton.Click += OnWalletButtonClicked;

        FadeInText();
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UserDashboardButton.Click -= OnDashboardButtonClicked;
        UserWalletButton.Click -= OnWalletButtonClicked;
    }
}