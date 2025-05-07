using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using BlockSense.Client;
using System;
using System.Threading.Tasks;

namespace BlockSense.Views;

public partial class MainWindow : Window
{
    private ContentControl _contentContainer;
    public static MainWindow Instance;
    public MainWindow()
    {
        InitializeComponent();
        _contentContainer = this.FindControl<ContentControl>("ContentContainer")!;
        Instance = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static void SetContent(UserControl newView)
    {
        Instance._contentContainer.Content = newView;
    }

    private async Task Animate(UserControl newView)
    {
        // Get the current content
        var currentContent = _contentContainer.Content;

        // Fade out current content
        if (currentContent != null)
        {
            await Animations.FadeOutAnimation.RunAsync(_contentContainer);
        }

        // Switch content
        _contentContainer.Content = newView;

        // Fade in new content
        await Animations.FadeInAnimation.RunAsync(_contentContainer);
    }

    public static async Task SwitchView(UserControl newView)
    {
        if (Instance == null)
        {
            throw new InvalidOperationException("MainWindow instance not available");
        }

        await Instance.Animate(newView);
    }
}
