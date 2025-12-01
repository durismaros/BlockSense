using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlockSense.Api;
using BlockSense.Client.TokenAuthentication;
using BlockSense.Client_Side.TokenAuthentication;
using BlockSense.Models.Invite;
using BlockSense.Models.TwoFactorAuth.BackupCode;
using BlockSense.Models.User;
using BlockSense.Services;
using BlockSense.Utilities;
using BlockSense.Utilities.Logging;
using BlockSense.Utilities.UI;
using BlockSense.ViewModels;
using BlockSense.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BlockSense;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Build the service provider
        var services = new ServiceCollection();

        services.AddSingleton<SystemUtils>();
        services.AddSingleton<AccessTokenManager>();
        services.AddSingleton<IViewSwitcher, ViewSwitcher>();

        // User related Models
        services.AddSingleton<UserInfo>();
        services.AddSingleton<AdditionalUserInfo>();
        services.AddSingleton<SystemIdentifier>();
        services.AddSingleton<UserInvites>();
        services.AddSingleton<TwoFactorBackupCodes>();

        services.AddSingleton<ProfilePictureHandler>();

        services.AddSingleton(new MainWindow());

        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7058/");
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
        services.AddTransient<PinEntryView>();
        services.AddTransient<BackupView>();
        services.AddTransient<SecretPhraseView>();
        services.AddTransient<MainWalletView>();

        services.AddTransient<TwoFactorSlidingPanel>();
        services.AddTransient<InviteManagerWindow>();

        Services = services.BuildServiceProvider();
        if (SystemUtils.AllocConsole())
            ConsoleLogger.Log("Console has been allocated");

        //ActivityLogger.InitializeApplicationLogger();
    }

    public async override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            Services is not null)
        {
            var systemUtils = Services.GetRequiredService<SystemUtils>();
            var viewSwitcher = Services.GetRequiredService<IViewSwitcher>();

            // Check server status
            //if (!await systemUtils.CheckServerStatus())
            //    return;

            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
            desktop.MainWindow.DataContext = Services.GetRequiredService<MainWindowViewModel>();
            //desktop.MainWindow.Show();

            // Continue with session check and navigation
            bool isSessionActive = await systemUtils.IsSessionActive();

            if (isSessionActive)
                await viewSwitcher.NavigateToAsync<WelcomeViewModel>();
            else
                await viewSwitcher.NavigateToAsync<MainViewModel>();
        }
    }
}
