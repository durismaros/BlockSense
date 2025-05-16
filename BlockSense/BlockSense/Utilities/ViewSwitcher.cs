using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using BlockSense.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Utilities
{
    public interface IViewSwitcher
    {
        Task NavigateToAsync<T>() where T : UserControl;
        Task NavigateToAsync(UserControl newView);
    }

    public class ViewSwitcher : IViewSwitcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MainWindow _mainWindow;

        public ViewSwitcher(IServiceProvider serviceProvider, MainWindow mainWindow)
        {
            _serviceProvider = serviceProvider;
            _mainWindow = mainWindow;
        }

        public async Task NavigateToAsync<T>() where T : UserControl
        {
            var view = _serviceProvider.GetRequiredService<T>();
            await SwitchViewAsync(view);
        }

        public async Task NavigateToAsync(UserControl newView)
        {
            await SwitchViewAsync(newView);
        }

        private async Task SwitchViewAsync(UserControl newView)
        {
            if (_mainWindow.CurrentContentContainer == null) return;

            if (_mainWindow.CurrentContentContainer.Content is UserControl oldView)
            {
                await AnimationManager.FadeOutAnimation.RunAsync(oldView);
            }

            newView.Opacity = 0;
            _mainWindow.CurrentContentContainer.Content = newView;
            await AnimationManager.FadeInAnimation.RunAsync(newView);
        }
    }
}
