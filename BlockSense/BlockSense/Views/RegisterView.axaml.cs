using System;
using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.TextFormatting.Unicode;
using BlockSense.Client;
using BlockSense.Server.User;
using BlockSense.Views;
using MySql.Data.MySqlClient;
using ZstdSharp.Unsafe;

namespace BlockSense;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
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
    private void HomeClick(object sender, RoutedEventArgs e)
    {
        Animations.AnimateTransition(this, new MainView());
    }

    /// <summary>
    /// 
    /// </summary>
    /// 
    private async void RegisterClick(object sender, RoutedEventArgs e)
    {
        if (User.Attempts >= 5)
        {
            SystemUtils.StartCheckTimer();
            ShowMessage("Try again later . . .");
            return;
        }

        string username = usernameRegister.Text?.Trim() ?? string.Empty;
        string email = emailRegister.Text?.Trim() ?? string.Empty;
        string password = passwordRegister.Text?.Trim() ?? string.Empty;
        string passwordConfirm = passwordConfirmRegister.Text?.Trim() ?? string.Empty;
        string invitationCode = invitationCodeRegister.Text?.Trim() ?? string.Empty;

        void ShowMessage(string message)
        {
            registerTextBorder.IsVisible = true;
            registerText.Text = message;
        }

        try
        {
            if (!InputHelper.Check(username, email, password, passwordConfirm, invitationCode))
                ShowMessage("Looks like you missed a required field");

            else if (password != passwordConfirm)
                ShowMessage("Passwords do not match");

            else
            {
                var (success, message) = await User.Register(username, email, password, invitationCode);
                if (success && !string.IsNullOrEmpty(message))
                {
                    ShowMessage(message);
                }
                else if (!success && !string.IsNullOrEmpty(message))
                {
                    User.Attempts++;
                    ShowMessage(message);
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Error: " + ex.Message);
        }
    }
}