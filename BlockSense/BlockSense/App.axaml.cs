using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BlockSense.ViewModels;
using BlockSense.Views;
using System.Runtime.InteropServices;
using System;
using MySqlX.XDevAPI;
using BlockSense.DB;
using Avalonia.Metadata;
using System.Reflection.Metadata;
using Avalonia.Controls;
using BlockSense.Client_Side.Token_authentication;
using Avalonia.DesignerSupport.Remote;
using BlockSense.Server_Based.Cryptography.Token_authentication.Refresh_Token;

namespace BlockSense;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public async override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        // Create the main window (or perform any other initialization)
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            ConsoleHelper.OpenConsole();    // Open the console at application startup
            await User.GetIPAddress();
            var (userId, refreshToken) = LocalRefreshToken.Retrieve();
            if (InputHelper.Check(userId, refreshToken))
            {
                if (await RemoteRefreshToken.Comparison(userId, refreshToken))
                {
                    await User.LoadUserInfo(userId);
                    desktop.MainWindow.Content = new Welcome();
                }
                else desktop.MainWindow.Content = new LoginView();
            }
        }
    }
}
