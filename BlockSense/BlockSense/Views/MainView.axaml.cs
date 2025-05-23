using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Client;
using BlockSense.Utilities;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml;

namespace BlockSense.Views;

public partial class MainView : UserControl
{
    private readonly IViewSwitcher _viewSwitcher;
    public MainView(IViewSwitcher viewSwitcher)
    {
        _viewSwitcher = viewSwitcher;
        InitializeComponent();
    }

    /// <summary>
    /// Redirects user to the Login page
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void LoginClick(object? sender, RoutedEventArgs e)
    {
        await _viewSwitcher.NavigateToAsync<LoginView>();
    }

    /// <summary>
    /// Redirects user to the Register page
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RegisterClick(object? sender, RoutedEventArgs e)
    {
        await _viewSwitcher.NavigateToAsync<RegisterView>();
    }
}
