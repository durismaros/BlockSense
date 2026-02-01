using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Utilities.UIComponents;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class MainWindow : Window
{
    public static MainWindow Instance
    {
        get;
        private set;
    } = default!;

    public MainWindow()
    {
        InitializeComponent();

        Instance = this;

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

    public async void ShowNotification(string title, string message)
    {
        var toast = new ToastNotification(title, message);

        NotificationStackPanel.Children.Add(toast);
        await Animations.FadeInAnimation.RunAsync(toast);

        await toast.ShowAsync();
    }
}