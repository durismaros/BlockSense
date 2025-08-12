using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System.Threading.Tasks;
using System;
using System.Diagnostics.Contracts;
using Avalonia.Interactivity;
using Org.BouncyCastle.Asn1.BC;
using BlockSense.Client;
using BlockSense.Views;
using Microsoft.Extensions.DependencyInjection;
using BlockSense.Utilities;

namespace BlockSense;

public partial class WelcomeView : UserControl
{
    private readonly IViewSwitcher _viewSwitcher;

    public WelcomeView(IViewSwitcher viewSwitcher)
    {
        _viewSwitcher = viewSwitcher;
        InitializeComponent();
        FadeInText();
    }

    public async void UserProfileClick(object sender, RoutedEventArgs e)
    {
        await _viewSwitcher.NavigateToAsync(new TestView());
    }
    public async void WalletClick(object sender, RoutedEventArgs e)
    {

        await _viewSwitcher.NavigateToAsync<UserProfileView>();
        //await _viewSwitcher.NavigateToAsync<PinEntryView>();
    }

    private async void FadeInText()
    {
        // Create and run fade-in animation
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(3),
            Easing = new SineEaseInOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(TextBlock.OpacityProperty, 1.0) }
                }
            }
        };

        await animation.RunAsync(WelcomeText);
        WelcomeText.Opacity = 1.0; // Set final opacity
    }
}