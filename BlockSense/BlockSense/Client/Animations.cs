using Avalonia.Animation;
using Avalonia.Styling;
using Avalonia.Controls;
using System;
namespace BlockSense.Client
{
    class Animations : Control
    {
        public static Animation FadeOutAnimation { get; set; } = new Animation();
        public static Animation FadeInAnimation { get; set; } = new Animation();


        public static void InitializeAnimations()
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
