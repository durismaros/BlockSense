using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlockSense.Desktop;

/// <summary>
/// The landing view of the application. Provides entry points to authentication
/// and account registration.
/// </summary>
public partial class WelcomeView : UserControl
{
    private readonly NavigationManager _navigationManager;

    /// <summary>
    /// Initialises a new instance of <see cref="WelcomeView"/> and resolves
    /// the <see cref="NavigationManager"/> from the application service provider.
    /// </summary>
    public WelcomeView()
    {
        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    /// <summary>
    /// Navigates to the <see cref="AuthenticationView"/> when the Authenticate
    /// button is clicked.
    /// </summary>
    private async void OnAuthenticateButtonClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<AuthenticationView>();
    }

    /// <summary>
    /// Navigates to the <see cref="RegistrationView"/> when the Create Account
    /// button is clicked.
    /// </summary>
    private async void OnRegisterButtonClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<RegistrationView>();
    }

    /// <summary>
    /// Subscribes to button click events when the view is attached to the visual tree.
    /// </summary>
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AuthenticateButton.Click += OnAuthenticateButtonClick;
        RegisterButton.Click += OnRegisterButtonClick;
    }

    /// <summary>
    /// Unsubscribes from button click events when the view is removed from the
    /// visual tree to prevent memory leaks.
    /// </summary>
    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AuthenticateButton.Click -= OnAuthenticateButtonClick;
        RegisterButton.Click -= OnRegisterButtonClick;
    }
}
