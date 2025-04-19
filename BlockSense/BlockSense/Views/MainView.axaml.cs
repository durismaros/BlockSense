using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Client;
using BlockSense.ViewModels;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
    private async void LoginClick(object? sender, RoutedEventArgs e)
    {
        await MainWindow.SwitchView(new LoginView());
    }

    /// <summary>
    /// Redirects user to the Register page
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RegisterClick(object? sender, RoutedEventArgs e)
    {
        await MainWindow.SwitchView(new RegisterView());
    }
}
