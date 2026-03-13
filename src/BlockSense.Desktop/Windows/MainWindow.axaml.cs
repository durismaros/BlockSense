using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

/// <summary>
/// The application shell window. Hosts the active view via <see cref="ContentContainer"/>,
/// displays toast notifications, and surfaces the 2FA and PIN entry sliding panels.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Gets the single running instance of <see cref="MainWindow"/>.
    /// Set once during construction and never replaced.
    /// </summary>
    public static MainWindow Instance { get; private set; } = default!;

    private readonly ILogger<MainWindow> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="MainWindow"/>, assigns the
    /// singleton reference, and registers the global input-throttle handler.
    /// </summary>
    public MainWindow()
    {
        _logger = App.ServiceProvider.GetRequiredService<ILogger<MainWindow>>();

        InitializeComponent();

        Instance = this;

        RegisterInputThrottleHandler();
    }

    /// <summary>
    /// Replaces the active view with <paramref name="newView"/>, applying a
    /// fade-out on the outgoing view and a fade-in on the incoming view.
    /// </summary>
    /// <param name="newView">The <see cref="UserControl"/> to display.</param>
    /// <returns>A <see cref="Task"/> that completes when the transition finishes.</returns>
    public async Task SwitchViewAsync(UserControl newView)
    {
        if (ContentContainer.Content is UserControl currentView)
        {
            _logger.LogDebug("Fading out {View}.", currentView.GetType().Name);
            await Animations.FadeOutAnimation.RunAsync(currentView);
        }

        newView.Opacity = 0;
        ContentContainer.Content = newView;

        _logger.LogDebug("Fading in {View}.", newView.GetType().Name);
        await Animations.FadeInAnimation.RunAsync(newView);
    }

    /// <summary>
    /// Creates and displays a toast notification with the given
    /// <paramref name="title"/> and <paramref name="message"/>.
    /// The notification removes itself automatically after its display duration elapses.
    /// </summary>
    /// <param name="title">Short heading shown at the top of the toast.</param>
    /// <param name="message">Body text of the toast.</param>
    public async void ShowNotification(string title, string message)
    {
        _logger.LogInformation("Showing notification — Title: '{Title}', Message: '{Message}'.", title, message);

        var toast = new ToastNotification(title, message);

        NotificationStackPanel.Children.Add(toast);
        await Animations.FadeInAnimation.RunAsync(toast);

        await toast.ShowAsync();
    }

    /// <summary>
    /// Registers a tunnelling pointer-pressed handler that suppresses events
    /// when the global <see cref="InputThrottler"/> determines they arrive too
    /// quickly in succession.
    /// </summary>
    private void RegisterInputThrottleHandler()
    {
        AddHandler(PointerPressedEvent, (sender, e) =>
        {
            if (!InputThrottler.ShouldProcess())
            {
                _logger.LogDebug("Pointer event suppressed by InputThrottler.");
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);
    }
}
