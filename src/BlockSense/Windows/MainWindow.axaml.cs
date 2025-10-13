using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using BlockSense.Utilities;
using BlockSense.Utilities.UI;
using System;
using System.Threading.Tasks;

namespace BlockSense.Views;

public partial class MainWindow : Window
{
    public ContentControl CurrentContentContainer { get; }

    public MainWindow()
    {
        InitializeComponent();
        CurrentContentContainer = this.FindControl<ContentControl>("ContentContainer")!;
        CurrentContentContainer.Content = new PlainView();

        // Pointer press handler
        this.AddHandler(InputElement.PointerPressedEvent, (sender, e) =>
        {
            if (!InputThrottler.ShouldProcess())
                e.Handled = true;
        }, RoutingStrategies.Tunnel);
    }
}
