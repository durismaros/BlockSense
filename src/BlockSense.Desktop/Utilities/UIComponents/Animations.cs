using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using System;

namespace BlockSense.Desktop.Utilities.UIComponents
{
    /// <summary>
    /// Provides reusable, preconfigured UI animations for common visual transitions across the BlockSense desktop application.
    /// </summary>
    public sealed class Animations : Control
    {
        /// <summary>
        /// Gets a predefined Fade-Out animation that transitions a control's opacity from fully visible to fully transparent.
        /// </summary>
        public static Animation FadeOutAnimation
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a predefined Fade-In animation that transitions a control's opacity from fully transparent to fully visible.
        /// </summary>
        public static Animation FadeInAnimation
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the static animation instances for the application.
        /// </summary>
        static Animations()
        {
            // Fade-out animation: Opacity 1.0 → 0.0
            FadeOutAnimation = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.35),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(0),
                        Setters = { new Setter(OpacityProperty, 1.0) }
                    },
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(0.35),
                        Setters = { new Setter(OpacityProperty, 0.0) }
                    }
                }
            };

            // Fade-in animation: Opacity 0.0 → 1.0
            FadeInAnimation = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.35),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(0),
                        Setters = { new Setter(OpacityProperty, 0.0) }
                    },
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(0.35),
                        Setters = { new Setter(OpacityProperty, 1.0) }
                    }
                }
            };
        }
    }
}
