using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using BlockSense.Desktop.Utilities.Validation;
using System;
using System.Threading;

namespace BlockSense.Desktop;

public partial class RegistrationView : UserControl
{
    private readonly NavigationManager _navigationManager;
    private readonly IUserService _userService;

    private CancellationTokenSource? _cancellationTokenSource;

    public RegistrationView(NavigationManager navigationManager, IUserService userService)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        InitializeComponent();

        HomeButton.Click += ToWelcomeViewClick;
        RegisterButton.Click += RegisterClick;
    }

    private async void ToWelcomeViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WelcomeView>();
    }

    private async void RegisterClick(object? sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        var request = new RegistrationRequest
        {
            Username = UsernameTextBox.Text?.Trim() ?? string.Empty,
            Email = EmailTextBox.Text?.Trim().ToLowerInvariant() ?? string.Empty,
            Password = PasswordTextBox.Text ?? string.Empty,
            InvitationCode = InvitationCodeTextBox.Text?.Trim() ?? string.Empty
        };


        if (!DataAnnotationsValidator.TryValidate(request, out var validationError))
        {
            ShowOutput(validationError);
            return;
        }

        if (request.Password != ConfirmPasswordTextBox.Text)
        {
            ShowOutput("Passwords Do Not Match");
            return;
        }

        ShowOutput("Registering . . .");
        var response = await _userService.RegisterAsync(request, cancellationToken);

        ShowOutput(response.Message);
    }

    /// <summary>
    /// Displays a message to the user in the output panel with a fade-in animation.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    private async void ShowOutput(string message)
    {
        OutputTextBlock.Text = message;
        OutputBorder.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(OutputBorder);
    }
}