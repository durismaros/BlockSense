using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Utilities.UIComponents
{
    /// <summary>
    /// Manages navigation between different <see cref="UserControl"/> views in the main application window.
    /// </summary>
    public sealed class NavigationManager
    {
        private readonly ILogger<NavigationManager> _logger;
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationManager"/> class.
        /// </summary>
        /// <param name="logger">The logger for navigation events.</param>
        /// <param name="mainWindow">Reference to the main application window that hosts views.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> or <paramref name="mainWindow"/> is null.</exception>
        public NavigationManager(ILogger<NavigationManager> logger, MainWindow mainWindow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        /// <summary>
        /// Navigates to a view of the specified type, resolving it from the application's service provider.
        /// </summary>
        /// <typeparam name="TView">The type of the <see cref="UserControl"/> to navigate to.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task NavigateToAsync<TView>() where TView : UserControl
        {
            var view = App.ServiceProvider.GetRequiredService<TView>();

            _logger.LogDebug("Navigating to `{View}`", nameof(view));
            await _mainWindow.SwitchViewAsync(view);
        }

        /// <summary>
        /// Navigates to the specified <see cref="UserControl"/> instance.
        /// </summary>
        /// <param name="view">The view instance to navigate to.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="view"/> is null.</exception>
        public async Task NavigateToAsync(UserControl view)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));

            _logger.LogDebug("Navigation to `{View}`", nameof(view));
            await _mainWindow.SwitchViewAsync(view);
        }
    }
}
