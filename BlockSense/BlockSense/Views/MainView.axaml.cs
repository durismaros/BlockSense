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
    private void LoginButton(object? sender, RoutedEventArgs e)
    {
        Animations.AnimateTransition(this, new LoginView());
    }

    /// <summary>
    /// Redirects user to the Register page
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RegisterButton(object? sender, RoutedEventArgs e)
    {
        Animations.AnimateTransition(this, new RegisterView());
    }
}
