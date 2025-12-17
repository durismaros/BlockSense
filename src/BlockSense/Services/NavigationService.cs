using Avalonia.Controls;
using BlockSense.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlockSense.Services
{
    /// <summary>
    /// Provides centralized navigation functionality for switching views within the <see cref="MainWindow"/>.
    /// </summary>
    public sealed class NavigationService
    {
        private readonly IServiceProvider _services;
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="services">The dependency injection service provider used to resolve view instances.</param>
        /// <param name="mainWindow">The main application window responsible for hosting and switching views.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="mainWindow"/> is null.</exception>
        public NavigationService(IServiceProvider services, MainWindow mainWindow)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        /// <summary>
        /// Navigates to a View of the specified type, resolving it from the dependency injection container.
        /// </summary>
        /// <typeparam name="TView">The type of <see cref="UserControl"/> to navigate to.</typeparam>
        public async void NavigateTo<TView>() where TView : UserControl
        {
            var view = _services.GetRequiredService<TView>();
            await _mainWindow.SwitchViewAsync(view);
        }

        /// <summary>
        /// Navigates to the specified View instance.
        /// </summary>
        /// <param name="view">The <see cref="UserControl"/> instance to display.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is null.</exception>
        public async void NavigateTo(UserControl view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            await _mainWindow.SwitchViewAsync(view);
        }
    }
}
