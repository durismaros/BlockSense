using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using System;

namespace BlockSense.Desktop;

public partial class AuthenticationView : UserControl
{
    private readonly NavigationManager _navigationManager;
    private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

    public AuthenticationView(NavigationManager navigationManager, TwoFactorSlidingPanel twoFactorSlidingPanel)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _twoFactorSlidingPanel = twoFactorSlidingPanel ?? throw new ArgumentNullException(nameof(twoFactorSlidingPanel));

        InitializeComponent();

        if (Content is Panel panel)
        {
            panel.Children.Add(_twoFactorSlidingPanel);
        }
    }

    public async void GoHomeAsync(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WelcomeView>();
    }

    private void Border_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}