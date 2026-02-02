using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlockSense.Desktop.Providers.Implementations;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Implementations;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling;
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
        public static IServiceProvider ServiceProvider
        {
            get;
            private set;
        }
        = default!;

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

                await ServiceProvider.GetRequiredService<IUserService>().InitializeAsync();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });

            services.AddTransient<AuthorizationHeaderHandler>();
            services.AddTransient<DeviceContextHeaderHandler>();

            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7262");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<AuthorizationHeaderHandler>()
            .AddHttpMessageHandler<DeviceContextHeaderHandler>();

            // --- Model Providers ---
            services.AddSingleton<IDeviceContextProvider, DeviceContextProvider>();
            services.AddSingleton<IRefreshTokenProvider, RefreshTokenProvider>();
            services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
            services.AddSingleton<ICurrentUserProvider, CurrentUserProvider>();

            // --- Services / Helpers ---
            services.AddSingleton<NavigationManager>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            services.AddScoped<ITokenService, TokenService>();

            // --- Views ---
            services.AddSingleton<WelcomeView>();
            services.AddSingleton<RegistrationView>();
            services.AddSingleton<AuthenticationView>();
            services.AddSingleton<TwoFactorSlidingPanel>();
            services.AddSingleton<HomeView>();
            services.AddSingleton<UserDashboardView>();

            // --- Windows ---
            services.AddSingleton<MainWindow>();
            services.AddSingleton<InvitationManagerWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}