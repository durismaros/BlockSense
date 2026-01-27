using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog;
using System;
using System.Threading;

namespace BlockSense.Desktop;

public partial class HomeView : UserControl
{
    private readonly NavigationManager _navigationManager;
    private readonly IApiClient _apiClient;

    private CancellationTokenSource? _cancellationTokenSource;

    public HomeView(NavigationManager navigationManager, IApiClient apiClient)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

        InitializeComponent();
        FadeInText();

        UserDashboardButton.Click += ToUserDashboardViewClick;
        UserWalletButton.Click += ToUserWalletViewClick;
    }

    private async void ToUserDashboardViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<UserDashboardView>();
    }

    private async void ToUserWalletViewClick(object? sender, RoutedEventArgs e)
    {

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
}