using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Utilities.UIComponents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class ToastNotification : UserControl
{
    public ToastNotification(string title, string message)
    {
        InitializeComponent();

        TitleText.Text = title;
        MessageText.Text = message;
    }

    public async Task ShowAsync()
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(4),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 0d)
                    }
                }
            }
        };

        await animation.RunAsync(ProgressBar, CancellationToken.None);

        await Animations.FadeOutAnimation.RunAsync(this);
        (Parent as Panel)?.Children.Remove(this);
    }
}