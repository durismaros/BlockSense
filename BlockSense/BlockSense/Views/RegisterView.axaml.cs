using System;
using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlockSense.DB;
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


    /// <summary>
    /// Returns back to the MainView
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HomeClick(object sender, RoutedEventArgs e)
    {
        Content = new MainView();
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

        void ShowMessage(string message)
        {
            registerTextBorder.IsVisible = true;
            registerText.Text = message;
        }

        try
        {
            if (!InputHelper.Check(username, email, password, passwordConfirm, invitationCode))
                ShowMessage("Looks like you missed a required field.");

            if (password != passwordConfirm)
                ShowMessage("The passwords you entered don’t match.");

            var (success, message) = await User.RegisterUser(username, email, password, invitationCode);
            if (message != null) ShowMessage(message);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Error: " + ex.Message);
        }
    }
}