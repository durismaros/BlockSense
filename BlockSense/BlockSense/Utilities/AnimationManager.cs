using Avalonia.Animation;
using Avalonia.Styling;
using Avalonia.Controls;
using System;
namespace BlockSense.Utilities
{
    class AnimationManager : Control
    {
        public static Animation FadeOutAnimation { get; }
        public static Animation FadeInAnimation { get; }


        static AnimationManager()
        {
            // Create animations once
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


            ConsoleHelper.Log("Animations initialized");
        }
    }
}
