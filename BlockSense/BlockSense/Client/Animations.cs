using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client
{
    class Animations : UserControl
    {   
        public static async void AnimateTransition(UserControl content, UserControl newView)
        {
            content.Content = newView;

            // Define the fade-in animation with easing
            var fadeInAnimation = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                Children =
        {
            new KeyFrame
            {
                Cue = new Cue(0),
                Setters = { new Setter(OpacityProperty, 0.0) }
            },
            new KeyFrame
            {
                Cue = new Cue(1),
                Setters = { new Setter(OpacityProperty, 1.0) },
                KeySpline = new KeySpline(0.4, 0, 0.2, 1) // Smooth easing
            }
        }
            };


            // Run the animation on the new view
            await fadeInAnimation.RunAsync(newView);
        }
    }
}
