using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using BlockSense.Desktop.Utilities.Validation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace BlockSense.Desktop;

public partial class RegistrationView : UserControl
{
    private readonly IUserService _userService;
    private readonly NavigationManager _navigationManager;

    private CancellationTokenSource? _cancellationTokenSource;

    public RegistrationView()
    {
        _userService = App.ServiceProvider.GetRequiredService<IUserService>()
            ?? throw new ArgumentNullException(nameof(IUserService));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        InitializeComponent();

        HomeButton.Click += ToWelcomeViewClick;
        RegisterButton.Click += RegisterClick;
        RevealPasswordButton.Click += RevealPasswordClick;
        RevealRepeatedPasswordButton.Click += RevealRepeatedPasswordClick;
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

        if (request.Password != RepeatPasswordTextBox.Text)
        {
            ShowOutput("Passwords Do Not Match");
            return;
        }

        ShowOutput("Registering . . .");
        var response = await _userService.RegisterAsync(request, cancellationToken);

        ShowOutput(response.Message);
    }

    private async void RevealPasswordClick(object? sender, RoutedEventArgs e)
    {
        PasswordTextBox.PasswordChar = EyeCrossLine1.IsVisible ? '●' : '\0';

        if (EyeCrossLine1.IsVisible)
        {
            // Password revealed → remove the cross line
            await Animations.FadeOutAnimation.RunAsync(EyeCrossLine1);
            EyeCrossLine1.IsVisible = false;
        }
        else
        {
            // Password hidden → show the cross line
            EyeCrossLine1.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(EyeCrossLine1);
        }
    }

    private async void RevealRepeatedPasswordClick(object? sender, RoutedEventArgs e)
    {
        RepeatPasswordTextBox.PasswordChar = EyeCrossLine2.IsVisible ? '●' : '\0';

        if (EyeCrossLine2.IsVisible)
        {
            // Password revealed → remove the cross line
            await Animations.FadeOutAnimation.RunAsync(EyeCrossLine2);
            EyeCrossLine2.IsVisible = false;
        }
        else
        {
            // Password hidden → show the cross line
            EyeCrossLine2.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(EyeCrossLine2);
        }
    }

    /// <summary>
    /// Displays a message to the user in the output panel with a fade-in animation.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    private async void ShowOutput(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (OutputTextBlock.Text == message)
            return;

        OutputTextBlock.Text = message;
        await Animations.FadeInAnimation.RunAsync(OutputTextBlock);

        if (OutputBorder.IsVisible)
            return;

        OutputBorder.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(OutputBorder);
    }
}