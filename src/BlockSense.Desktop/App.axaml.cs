using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlockSense.Desktop.Services.Implementations;
using BlockSense.Desktop.Services.Interfaces;
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
        } = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            ConfigureServices();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();

                var _ = ServiceProvider.GetRequiredService<NavigationManager>().NavigateToAsync<WelcomeView>();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });

            services.AddSingleton<IApiClient, ApiClient>();

            services.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7147/");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

            // --- Services / Helpers ---
            services.AddSingleton<NavigationManager>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();

            // --- Views ---
            services.AddSingleton<WelcomeView>();
            services.AddSingleton<RegistrationView>();
            services.AddSingleton<AuthenticationView>();
            services.AddSingleton<TwoFactorSlidingPanel>();

            // --- Windows ---
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}