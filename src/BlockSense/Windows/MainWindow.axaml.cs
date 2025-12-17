using Avalonia.Controls;
using BlockSense.Utilities.UI;
using System.Threading.Tasks;

namespace BlockSense.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public async Task SwitchViewAsync(UserControl newView)
    {
        if (ContentContainer.Content is UserControl oldView)
        {
            await AnimationManager.FadeOutAnimation.RunAsync(oldView);
        }

        newView.Opacity = 0;
        ContentContainer.Content = newView;

        await AnimationManager.FadeInAnimation.RunAsync(newView);
    }
}
