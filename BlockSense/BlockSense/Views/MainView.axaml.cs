using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.ViewModels;
using System;
using System.Xml;

namespace BlockSense.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Redirects user to the Login page
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LoginButton(object? sender, RoutedEventArgs e)
    {
        Content = new LoginView();
    }


    /// <summary>
    /// Redirects user to the Register page
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RegisterButton(object? sender, RoutedEventArgs e)
    {
        Content = new RegisterView();
    }
}
