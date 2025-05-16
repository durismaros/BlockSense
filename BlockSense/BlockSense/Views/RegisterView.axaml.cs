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
    private readonly MainView _mainView;
    public RegisterView(UserService userService, MainView mainView)
    {
        _userService = userService;
        _mainView = mainView;
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
        //await MainWindow.SwitchView(_mainView);
    }

    /// <summary>
    /// 
    /// </summary>
    /// 
    private async void RegisterClick(object sender, RoutedEventArgs e)
    {
        string username = usernameRegister.Text?.Trim() ?? string.Empty;
        string email = emailRegister.Text?.Trim() ?? string.Empty;
        string password = passwordRegister.Text?.Trim() ?? string.Empty;
        string passwordConfirm = passwordConfirmRegister.Text?.Trim() ?? string.Empty;
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
            if (!InputHelper.Check(username, email, password, passwordConfirm, invitationCode))
                ShowMessage("Looks like you missed a required field");

            else if (password != passwordConfirm)
                ShowMessage("Passwords do not match");

            else if (Zxcvbn.Core.EvaluatePassword(password).Score < 3)
                ShowMessage("Too weak! Try a stronger password");


            else
            {
                var newRegisterRequest = new RegisterRequestModel(username, email, password, invitationCode);
                var (success, message) = await _userService.Register(newRegisterRequest);
                if (success)
                    ShowMessage(message);

                else
                {
                    ShowMessage(message);
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.Log("Error: " + ex.Message);
        }
    }
}