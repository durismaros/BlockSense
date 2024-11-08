using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlockSense.Views;

namespace BlockSense;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Returns back to the MainView
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HomeClick(object sender, RoutedEventArgs e)
    {
        Content = new MainView();
    }
}