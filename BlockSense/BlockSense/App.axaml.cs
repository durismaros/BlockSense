using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlockSense.Views;
using System.Runtime.InteropServices;
using System;
using MySqlX.XDevAPI;
using Avalonia.Metadata;
using System.Reflection.Metadata;
using Avalonia.Controls;
using BlockSense.Client_Side.Token_authentication;
using Avalonia.DesignerSupport.Remote;
using NBitcoin;
using Microsoft.Extensions.DependencyInjection;
using BlockSense.Models;
using BlockSense.Models.User;
using BlockSense.Client.Token_authentication;
using BlockSense.Api;
using BlockSense.Utilities;
using BlockSense.Services;
using System.Collections.Generic;

namespace BlockSense;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Build the service provider
        var services = new ServiceCollection();

        services.AddSingleton<SystemUtils>();
        services.AddSingleton<AccessTokenManager>();
        services.AddSingleton<IViewSwitcher, ViewSwitcher>();

        // User related Models
        services.AddSingleton<UserInfoModel>();
        services.AddSingleton<AdditionalUserInfoModel>();
        services.AddSingleton<SystemIdentifierModel>();
        services.AddSingleton<List<InviteInfoModel>>();

        services.AddSingleton<ProfilePictureHandler>();

        services.AddSingleton(new MainWindow());

        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://unicorn-casual");
        }).AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddTransient<UserService>();
        services.AddTransient<AuthHeaderHandler>();
        services.AddTransient<RefreshTokenManager>();
        services.AddTransient<TwoFactorAuthService>();

        services.AddTransient<MainView>();
        services.AddTransient<LoginView>();
        services.AddTransient<RegisterView>();
        services.AddTransient<WelcomeView>();
        services.AddTransient<UserProfileView>();
        services.AddTransient<InviteManagerWindow>();

        Services = services.BuildServiceProvider();
        if (SystemUtils.AllocConsole())
            ConsoleHelper.Log("Console has been allocated");

        //ActivityLogger.InitializeApplicationLogger();
    }

    public async override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();

            var systemUtils = Services.GetRequiredService<SystemUtils>();

            bool isSessionActive = await systemUtils.IsSessionActive();

            //UserControl initView = isSessionActive ? new WelcomeView() : new MainView();
            await Services.GetRequiredService<IViewSwitcher>().NavigateToAsync<WelcomeView>();
        }
    }
}
