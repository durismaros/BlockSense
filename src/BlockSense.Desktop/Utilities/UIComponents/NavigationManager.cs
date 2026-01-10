using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Utilities.UIComponents
{
    public sealed class NavigationManager
    {
        private readonly ILogger<NavigationManager> _logger;
        private readonly MainWindow _mainWindow;

        public NavigationManager(ILogger<NavigationManager> logger, MainWindow mainWindow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        public async Task NavigateToAsync<TView>() where TView : UserControl
        {
            var view = App.ServiceProvider.GetRequiredService<TView>();

            _logger.LogDebug("Navigating to `{View}`", view);
            await _mainWindow.SwitchViewAsync(view);
        }

        public async Task NavigateToAsync(UserControl view)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));

            _logger.LogDebug("Navigation to `{View}`", view.Name);
            await _mainWindow.SwitchViewAsync(view);
        }
    }
}
