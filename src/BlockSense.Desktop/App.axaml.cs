using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlockSense.Desktop.Providers.Implementations;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Implementations;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling.HeaderHandlers;
using BlockSense.Desktop.Utilities.FileManagement;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Net.Http.Headers;

namespace BlockSense.Desktop
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = default!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            ConfigureServices();
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();

                var session = ServiceProvider.GetRequiredService<ISessionService>();
                await session.InitializeSessionAsync();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });

            // HTTP Client with handlers
            services.AddTransient<AuthorizationHeaderHandler>();
            services.AddTransient<DeviceContextHeaderHandler>();

            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://unicorn-casual-yeti.ngrok-free.app");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<AuthorizationHeaderHandler>()
            .AddHttpMessageHandler<DeviceContextHeaderHandler>();

            // --- Model Providers (Singleton for state management) ---
            services.AddSingleton<IDeviceContextProvider, DeviceContextProvider>();
            services.AddSingleton<IRefreshTokenProvider, RefreshTokenProvider>();
            services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
            services.AddSingleton<ICurrentUserProvider, CurrentUserProvider>();
            services.AddSingleton<IWalletProvider, WalletProvider>();

            // --- Infrastructure ---
            services.AddSingleton<NavigationManager>();

            // --- Services / Helpers ---
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            services.AddScoped<IWalletService, WalletService>();

            // --- Views ---
            services.AddSingleton<WelcomeView>();
            services.AddSingleton<RegistrationView>();
            services.AddSingleton<AuthenticationView>();
            services.AddSingleton<HomeView>();
            services.AddSingleton<UserDashboardView>();
            services.AddSingleton<WalletSelectionView>();
            services.AddSingleton<RecoveryPhraseView>();
            services.AddSingleton<RecoveryPhraseImportView>();
            services.AddSingleton<PinEntryView>();
            services.AddSingleton<CryptoWalletView>();

            // --- Panels ---
            services.AddSingleton<TwoFactorSlidingPanel>();
            services.AddSingleton<PinEntrySlidingPanel>();

            // --- Windows ---
            services.AddSingleton<MainWindow>();
            services.AddSingleton<InvitationManagerWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}