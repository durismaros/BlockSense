using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;
using BlockSense.ViewModels;
using System;

namespace BlockSense.Views;

public partial class WelcomeView : UserControl
{
    public WelcomeView()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttached;
    }

    private async void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is WelcomeViewModel vm)
        {
            vm.RequestFadeIn += FadeInText;
        }
    }

    private async void FadeInText()
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(3),
            Easing = new SineEaseInOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1.0)
                    }
                }
            }
        };

        await animation.RunAsync(WelcomeText);
    }
}
