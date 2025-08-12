using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using BlockSense.Utilities;
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
    }
}
