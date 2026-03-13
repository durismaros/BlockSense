using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using BlockSense.Desktop.Utilities.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

/// <summary>
/// View that presents the registration form and orchestrates the account-creation flow.
/// </summary>
public partial class RegistrationView : UserControl
{
    private readonly IUserService _userService;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<RegistrationView> _logger;

    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Initialises a new instance of <see cref="RegistrationView"/>.
    /// </summary>
    public RegistrationView()
    {
        _userService = App.ServiceProvider.GetRequiredService<IUserService>()
            ?? throw new ArgumentNullException(nameof(IUserService));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _logger = App.ServiceProvider.GetRequiredService<ILogger<RegistrationView>>()
            ?? throw new ArgumentNullException(nameof(ILogger<RegistrationView>));

        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _cancellationTokenSource?.Cancel();

        HomeButton.Click += OnHomeButtonClicked;
        RegisterButton.Click += OnRegisterButtonClicked;
        RevealPasswordButton.Click += OnRevealPasswordButtonClicked;
        RevealRepeatedPasswordButton.Click += OnRevealRepeatedPasswordButtonClicked;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        ResetForm();

        HomeButton.Click -= OnHomeButtonClicked;
        RegisterButton.Click -= OnRegisterButtonClicked;
        RevealPasswordButton.Click -= OnRevealPasswordButtonClicked;
        RevealRepeatedPasswordButton.Click -= OnRevealRepeatedPasswordButtonClicked;

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
    /// Validates inputs and submits a registration request.
    /// </summary>
    private async void OnRegisterButtonClicked(object? sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        var request = BuildRegistrationRequest();

        if (!DataAnnotationsValidator.TryValidate(request, out var validationError))
        {
            _logger.LogWarning("Registration validation failed: {Error}", validationError);
            MainWindow.Instance.ShowNotification("Registration", validationError);
            return;
        }

        if (!PasswordsMatch())
        {
            _logger.LogWarning("Registration failed: passwords do not match.");
            MainWindow.Instance.ShowNotification("Registration", "Oops! Passwords do not match.");
            return;
        }

        _logger.LogInformation("Submitting registration request for username '{Username}'.", request.Username);
        await _userService.RegisterAsync(request, cancellationToken);
    }

    /// <summary>
    /// Toggles the password field between masked and plain-text display.
    /// </summary>
    private async void OnRevealPasswordButtonClicked(object? sender, RoutedEventArgs e)
    {
        await TogglePasswordVisibilityAsync(PasswordTextBox, EyeCrossLine);
    }

    /// <summary>
    /// Toggles the repeat-password field between masked and plain-text display.
    /// </summary>
    private async void OnRevealRepeatedPasswordButtonClicked(object? sender, RoutedEventArgs e)
    {
        await TogglePasswordVisibilityAsync(RepeatPasswordTextBox, RepeatedEyeCrossLine);
    }

    private RegistrationRequest BuildRegistrationRequest() => new()
    {
        Username = UsernameTextBox.Text?.Trim() ?? string.Empty,
        Email = EmailTextBox.Text?.Trim().ToLowerInvariant() ?? string.Empty,
        Password = PasswordTextBox.Text ?? string.Empty,
        InvitationCode = InvitationCodeTextBox.Text?.Trim() ?? string.Empty
    };

    private bool PasswordsMatch() =>
        PasswordTextBox.Text == RepeatPasswordTextBox.Text;

    private static async Task TogglePasswordVisibilityAsync(
        TextBox passwordBox,
        Avalonia.Controls.Shapes.Path crossLineIcon)
    {
        bool isCurrentlyRevealed = crossLineIcon.IsVisible;

        passwordBox.PasswordChar = isCurrentlyRevealed ? '●' : '\0';

        if (isCurrentlyRevealed)
        {
            await Animations.FadeOutAnimation.RunAsync(crossLineIcon);
            crossLineIcon.IsVisible = false;
        }
        else
        {
            crossLineIcon.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(crossLineIcon);
        }
    }

    private void ResetForm()
    {
        UsernameTextBox.Text = string.Empty;
        EmailTextBox.Text = string.Empty;
        PasswordTextBox.Text = string.Empty;
        RepeatPasswordTextBox.Text = string.Empty;
        InvitationCodeTextBox.Text = string.Empty;

        EyeCrossLine.IsVisible = false;
        RepeatedEyeCrossLine.IsVisible = false;
    }
}
