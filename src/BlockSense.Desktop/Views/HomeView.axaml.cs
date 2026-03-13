using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace BlockSense.Desktop;

/// <summary>
/// The main landing view shown after the user is authenticated.
/// Provides navigation to the user dashboard and the wallet.
/// </summary>
public partial class HomeView : UserControl
{
    private readonly IWalletService _walletService;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<HomeView> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="HomeView"/>.
    /// </summary>
    public HomeView()
    {
        _walletService = App.ServiceProvider.GetRequiredService<IWalletService>()
            ?? throw new ArgumentNullException(nameof(IWalletService));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _logger = App.ServiceProvider.GetRequiredService<ILogger<HomeView>>()
            ?? throw new ArgumentNullException(nameof(ILogger<HomeView>));

        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UserDashboardButton.Click += OnUserDashboardButtonClicked;
        UserWalletButton.Click += OnUserWalletButtonClicked;

        AnimateWelcomeText();
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UserDashboardButton.Click -= OnUserDashboardButtonClicked;
        UserWalletButton.Click -= OnUserWalletButtonClicked;
    }

    /// <summary>
    /// Navigates to the user dashboard view.
    /// </summary>
    private async void OnUserDashboardButtonClicked(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<UserDashboardView>();
    }

    /// <summary>
    /// Navigates to the wallet view, or to wallet selection if no wallet exists.
    /// </summary>
    private async void OnUserWalletButtonClicked(object? sender, RoutedEventArgs e)
    {
        bool walletExists = await _walletService.WalletExistsAsync();

        if (walletExists)
        {
            _logger.LogInformation("Existing wallet found — unlocking.");
            await _walletService.UnlockWalletAsync();
        }
        else
        {
            _logger.LogInformation("No wallet found — navigating to wallet selection.");
            await _navigationManager.NavigateToAsync<WalletSelectionView>();
        }
    }

    /// <summary>
    /// Runs a slow sine-eased fade-in on the welcome text block.
    /// </summary>
    private async void AnimateWelcomeText()
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(3),
            Easing = new SineEaseInOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue     = new Cue(1),
                    Setters = { new Setter(TextBlock.OpacityProperty, 1.0) }
                }
            }
        };

        await animation.RunAsync(WelcomeText);
        WelcomeText.Opacity = 1.0;
    }
}
