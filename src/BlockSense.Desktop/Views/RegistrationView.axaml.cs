using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        RegisterButton.Click += RegisterAsync;
    }

    public async void GoHomeAsync(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WelcomeView>();
    }

    private async void RegisterAsync(object? sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        string username = UsernameTextBox.Text?.Trim() ?? string.Empty;
        string email = EmailTextBox.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        string password = PasswordTextBox.Text ?? string.Empty;
        string confirmPassword = ConfirmPasswordTextBox.Text ?? string.Empty;
        string invitationCode = InvitationCodeTextBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) ||
            string.IsNullOrEmpty(invitationCode))
        {
            ShowOutput("Looks like you missed a required field");
            return;
        }

        if (password != confirmPassword)
        {
            ShowOutput("Passwords do not match.");
            return;
        }


        var request = new RegistrationRequest
        {
            Username = username,
            Email = email,
            Password = password,
            InvitationCode = invitationCode
        };

        if (!ValidateRegistrationRequest(request, out var validationError))
        {
            ShowOutput(validationError);
            return;
        }

        var response = await _userService.RegisterAsync(request, cancellationToken);

        var message = ResultCodeToMessage(response);

        ShowOutput(message);
    }

    private async void ShowOutput(string message)
    {
        OutputTextBlock.Text = message;
        OutputBorder.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(OutputBorder);
    }

    private bool ValidateRegistrationRequest(RegistrationRequest request, out string error)
    {
        var results = new List<ValidationResult>();
        error = Validator.TryValidateObject(request, new(request), results, true)
            ? ""
            : results[0].ErrorMessage ?? "Invalid input.";
        return error == "";
    }

    private string ResultCodeToMessage(string resultCode)
    {
        return resultCode switch
        {
            // Registration
            ResultCodes.Registration.InvalidInvitation => "The invitation code is invalid.",
            ResultCodes.Registration.UsernameTaken => "That username is already taken.",
            ResultCodes.Registration.EmailTaken => "That email is already registered.",
            ResultCodes.Registration.RegistrationSuccess => "Registration successful!",

            // Generic
            ResultCodes.Generic.BadRequest => "Something went wrong with your request.",
            ResultCodes.Generic.InternalServerError => "The server encountered an error. Please try again later.",

            // Client
            ResultCodes.Client.Timeout => "Request timed out. Check your connection and try again.",
            ResultCodes.Client.NetworkError => "Network error. Please check your connection.",
            ResultCodes.Client.RequestCancelled => "The request was cancelled.",
            ResultCodes.Client.UnknownError => "An unknown error occurred. Please try again.",

            // Fallback
            _ => "An unexpected error occurred. Please try again."
        };
    }

}