using System;
using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.TextFormatting.Unicode;
using BlockSense.Models.Requests;
using BlockSense.Services;
using BlockSense.Utilities;
using BlockSense.Views;
using MySql.Data.MySqlClient;
using ZstdSharp.Unsafe;

namespace BlockSense;

public partial class RegisterView : UserControl
{
    private readonly UserService _userService;
    private readonly IViewSwitcher _viewSwitcher;
    public RegisterView(UserService userService, IViewSwitcher viewSwitcher)
    {
        _userService = userService;
        _viewSwitcher = viewSwitcher;
        InitializeComponent();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            RegisterButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
    }


    /// <summary>
    /// Returns back to the MainView
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HomeClick(object sender, RoutedEventArgs e)
    {
        await _viewSwitcher.NavigateToAsync<MainView>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RegisterClick(object sender, RoutedEventArgs e)
    {
        string username = usernameRegister.Text?.Trim() ?? string.Empty;
        string email = emailRegister.Text?.Trim() ?? string.Empty;
        string password = passwordRegister.Text?.Trim() ?? string.Empty;
        string passwordConfirmation = passwordConfirmRegister.Text?.Trim() ?? string.Empty;
        string invitationCode = invitationCodeRegister.Text?.Trim() ?? string.Empty;

        async void ShowMessage(string message)
        {
            if (!registerTextBorder.IsVisible || registerText.Text != message)
            {
                registerText.Text = message;
                registerTextBorder.IsVisible = true;
                await AnimationManager.FadeInAnimation.RunAsync(registerTextBorder);
            }
        }

        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwordConfirmation) || string.IsNullOrWhiteSpace(invitationCode))
            {
                ShowMessage("Looks like you missed a required field");
                return;
            }

            if (password != passwordConfirmation)
            {
                ShowMessage("Passwords do not match");
                return;
            }

            var request = new RegisterRequestModel(username, email, password, invitationCode);
            var response = await _userService.Register(request);

            if (response is null || response.Message is null)
                return;

            ShowMessage(response.Message);
        }
        catch (Exception ex)
        {
            ConsoleLogger.Log("Error: " + ex.Message);
        }
    }
}