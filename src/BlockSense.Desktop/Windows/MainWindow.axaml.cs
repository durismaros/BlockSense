using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Opened += OnOpened;

        // Pointer press handler
        this.AddHandler(InputElement.PointerPressedEvent, (sender, e) =>
        {
            if (!InputThrottler.ShouldProcess())
            {
                e.Handled = true;
            }

        }, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Replaces the current view in the main content container with a new <see cref="UserControl"/>, applying fade-out and fade-in animations for smooth visual transitions.
    /// </summary>
    /// <param name="newView">The new <see cref="UserControl"/> to display.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SwitchViewAsync(UserControl newView)
    {
        if (ContentContainer.Content is UserControl oldView)
        {
            await Animations.FadeOutAnimation.RunAsync(oldView);
        }

        newView.Opacity = 0;
        ContentContainer.Content = newView;

        await Animations.FadeInAnimation.RunAsync(newView);
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        var navigation = App.ServiceProvider.GetRequiredService<NavigationManager>();
        await navigation.NavigateToAsync<WelcomeView>();
    }
}