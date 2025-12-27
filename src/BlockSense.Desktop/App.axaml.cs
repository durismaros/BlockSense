using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlockSense.Desktop.Utilities.FileManagement;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

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

            // --- Services / Helpers ---
            services.AddSingleton<DirectoryStructure>();

            services.AddSingleton<NavigationManager>();

            // --- Views ---
            services.AddSingleton<WelcomeView>();
            services.AddSingleton<RegistrationView>();
            services.AddSingleton<AuthenticationView>();

            // --- Windows ---
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}