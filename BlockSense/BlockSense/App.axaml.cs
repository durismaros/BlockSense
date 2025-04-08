using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlockSense.ViewModels;
using BlockSense.Views;
using System.Runtime.InteropServices;
using System;
using MySqlX.XDevAPI;
using Avalonia.Metadata;
using System.Reflection.Metadata;
using Avalonia.Controls;
using BlockSense.Client_Side.Token_authentication;
using Avalonia.DesignerSupport.Remote;
using BlockSense.Server_Based.Cryptography.Token_authentication.Refresh_Token;
using BlockSense.Server_Based.Cryptography;
using BlockSense.Client;
using BlockSense.Client.Utilities;
using BlockSense.Server.Cryptography.Hashing;
using NBitcoin;
using BlockSense.Client.Identifiers;

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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            SystemUtils.AllocConsole();
            DirStructure.InitializeSecureStorage();
            desktop.MainWindow = new MainWindow();
            await NetworkIdentifier.GetIpAddress();
            desktop.MainWindow.Content = (await SystemUtils.IsSessionActive()) ? new Welcome() : new MainView();
        }
    }
}
