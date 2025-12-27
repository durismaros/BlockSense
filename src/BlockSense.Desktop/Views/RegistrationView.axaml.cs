using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using System;

namespace BlockSense.Desktop;

public partial class RegistrationView : UserControl
{
    private readonly NavigationManager _navigationManager;

    public RegistrationView(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

        InitializeComponent();
    }

    public async void GoHomeAsync(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WelcomeView>();
    }
}