using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BlockSense.Desktop;

public partial class TestView : UserControl
{
    public TestView()
    {
        InitializeComponent();
    }

    private void AuthenticateButton_Holding(object? sender, Avalonia.Input.HoldingRoutedEventArgs e)
    {
    }
}