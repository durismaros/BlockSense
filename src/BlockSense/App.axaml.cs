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
using BlockSense.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlockSense;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Build the service provider
        var services = new ServiceCollection();

        // --- Views ---
        services.AddTransient<PlainView>();
        services.AddTransient<MainView>();
        services.AddTransient<LoginView>();
        services.AddTransient<RegisterView>();
        services.AddTransient<WelcomeView>();
        services.AddTransient<UserProfileView>();
        services.AddTransient<MainWalletView>();
        services.AddTransient<PinEntryView>();
        services.AddTransient<BackupView>();
        services.AddTransient<SecretPhraseView>();
        services.AddTransient<TwoFactorSlidingPanel>();


        // --- Windows / Panels ---
        services.AddSingleton<MainWindow>();
        services.AddTransient<TwoFactorSlidingPanel>();
        services.AddTransient<InviteManagerWindow>();

        // --- Services ---
        services.AddSingleton<SystemUtils>();
        services.AddSingleton<NavigationService>(); // Central navigation
        services.AddTransient<UserService>();
        services.AddSingleton<AccessTokenManager>();
        services.AddTransient<RefreshTokenManager>();
        services.AddTransient<AuthHeaderHandler>();
        services.AddTransient<TwoFactorAuthService>();

        // --- User-related Models ---
        services.AddSingleton<UserInfo>();
        services.AddSingleton<AdditionalUserInfo>();
        services.AddSingleton<SystemIdentifier>();
        services.AddSingleton<UserInvites>();
        services.AddSingleton<TwoFactorBackupCodes>();
        services.AddSingleton<ProfilePictureHandler>();

        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7058/");
        }).AddHttpMessageHandler<AuthHeaderHandler>();

        Services = services.BuildServiceProvider();

        if (SystemUtils.AllocConsole())
            ConsoleLogger.Log("Console has been allocated");

        //ActivityLogger.InitializeApplicationLogger();
    }

    public async override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            Services is not null)
        {
            var navigation = Services.GetRequiredService<NavigationService>();
            var systemUtils = Services.GetRequiredService<SystemUtils>();

            desktop.MainWindow = new MainWindow()
            {
                DataContext = Services.GetRequiredService<MainWindow>()
            };

            // Check server status
            //if (!await systemUtils.CheckServerStatus())
            //    return;


            // Continue with session check and navigation
            bool isSessionActive = await systemUtils.IsSessionActive();
            if (isSessionActive)
                navigation.NavigateTo<WelcomeView>();
            else
                navigation.NavigateTo<MainView>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
