using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Utilities.UIComponents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

/// <summary>
/// A self-dismissing toast notification that displays a title and message,
/// animates a shrinking progress bar, then fades out and removes itself.
/// </summary>
public partial class ToastNotification : UserControl
{
    /// <summary>
    /// Initialises a new instance of <see cref="ToastNotification"/> with empty text.
    /// </summary>
    public ToastNotification()
    {
        InitializeComponent();

        TitleText.Text = string.Empty;
        MessageText.Text = string.Empty;
    }

    /// <summary>
    /// Initialises a new instance of <see cref="ToastNotification"/> with the
    /// specified <paramref name="title"/> and <paramref name="message"/>.
    /// </summary>
    /// <param name="title">Short heading displayed at the top of the toast.</param>
    /// <param name="message">Body text displayed below the title.</param>
    public ToastNotification(string title, string message)
    {
        InitializeComponent();

        TitleText.Text = title;
        MessageText.Text = message;
    }

    /// <summary>
    /// Animates the progress bar from full width to zero over four seconds,
    /// then fades out and removes the toast from its parent panel.
    /// </summary>
    public async Task ShowAsync()
    {
        await new Animation
        {
            Duration = TimeSpan.FromSeconds(4),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue     = new Cue(0),
                    Setters = { new Setter(ScaleTransform.ScaleXProperty, 1d) }
                },
                new KeyFrame
                {
                    Cue     = new Cue(1),
                    Setters = { new Setter(ScaleTransform.ScaleXProperty, 0d) }
                }
            }
        }.RunAsync(ProgressBar, CancellationToken.None);

        await Animations.FadeOutAnimation.RunAsync(this);
        (Parent as Panel)?.Children.Remove(this);
    }
}
