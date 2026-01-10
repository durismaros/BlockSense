using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Pointer press handler
        this.AddHandler(InputElement.PointerPressedEvent, (sender, e) =>
        {
            if (!InputThrottler.ShouldProcess())
                e.Handled = true;
        }, RoutingStrategies.Tunnel);
    }

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
}