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

namespace BlockSense;

public partial class Welcome : UserControl
{
    public Welcome()
    {
        InitializeComponent();
        FadeInText();
    }

    public void UserProfileClick(object sender, RoutedEventArgs e)
    {
        Animations.AnimateTransition(this, new UserProfile());
    }
    public void WalletClick(object sender, RoutedEventArgs e)
    {
        Animations.AnimateTransition(this, new BackupView());
    }

    private async void FadeInText()
    {
        var welcomeText = this.FindControl<TextBlock>("WelcomeText");

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

        await animation.RunAsync(welcomeText!);
        welcomeText!.Opacity = 1.0; // Set final opacity
    }
}