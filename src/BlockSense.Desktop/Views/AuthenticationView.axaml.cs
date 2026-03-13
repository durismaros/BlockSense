using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using BlockSense.Desktop.Utilities.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace BlockSense.Desktop;

/// <summary>
/// View that presents the login form and orchestrates the authentication flow.
/// </summary>
public partial class AuthenticationView : UserControl
{
    private readonly IAuthService _authService;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<AuthenticationView> _logger;

    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Initialises a new instance of <see cref="AuthenticationView"/>.
    /// </summary>
    public AuthenticationView()
    {
        _authService = App.ServiceProvider.GetRequiredService<IAuthService>()
            ?? throw new ArgumentNullException(nameof(IAuthService));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _logger = App.ServiceProvider.GetRequiredService<ILogger<AuthenticationView>>()
            ?? throw new ArgumentNullException(nameof(ILogger<AuthenticationView>));

        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _cancellationTokenSource?.Cancel();

        HomeButton.Click += OnHomeButtonClicked;
        AuthenticateButton.Click += OnAuthenticateButtonClicked;
        RevealPasswordButton.Click += OnRevealPasswordButtonClicked;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        ResetForm();

        HomeButton.Click -= OnHomeButtonClicked;
        AuthenticateButton.Click -= OnAuthenticateButtonClicked;
        RevealPasswordButton.Click -= OnRevealPasswordButtonClicked;

        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Navigates back to the <see cref="WelcomeView"/>.
    /// </summary>
    private async void OnHomeButtonClicked(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WelcomeView>();
    }

    /// <summary>
    /// Validates inputs and submits an authentication request.
    /// </summary>
    private async void OnAuthenticateButtonClicked(object? sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        var request = BuildAuthRequest();

        if (!DataAnnotationsValidator.TryValidate(request, out var validationError))
        {
            _logger.LogWarning("Authentication validation failed: {Error}", validationError);
            MainWindow.Instance.ShowNotification("Authentication", validationError);
            return;
        }

        _logger.LogInformation("Submitting authentication request for login '{Login}'.", request.Login);
        await _authService.AuthenticateAsync(request, cancellationToken);
    }

    /// <summary>
    /// Toggles the password field between masked and plain-text display.
    /// </summary>
    private async void OnRevealPasswordButtonClicked(object? sender, RoutedEventArgs e)
    {
        bool isCurrentlyRevealed = EyeCrossLine.IsVisible;

        PasswordTextBox.PasswordChar = isCurrentlyRevealed ? '●' : '\0';

        if (isCurrentlyRevealed)
        {
            await Animations.FadeOutAnimation.RunAsync(EyeCrossLine);
            EyeCrossLine.IsVisible = false;
        }
        else
        {
            EyeCrossLine.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(EyeCrossLine);
        }
    }

    /// <summary>
    /// Constructs an <see cref="AuthRequest"/> from the current input field values.
    /// </summary>
    private AuthRequest BuildAuthRequest() => new()
    {
        Login = LoginTextBox.Text?.Trim() ?? string.Empty,
        Password = PasswordTextBox.Text ?? string.Empty
    };

    /// <summary>
    /// Clears all input fields and resets the password-reveal icon.
    /// </summary>
    private void ResetForm()
    {
        LoginTextBox.Text = string.Empty;
        PasswordTextBox.Text = string.Empty;
        EyeCrossLine.IsVisible = false;
    }
}
