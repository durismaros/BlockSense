using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Client;
using BlockSense.Server.Cryptography.Wallet;
using BlockSense.Views;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockSense;

public partial class SecretPhraseView : UserControl
{
    private bool _isPanelVisible = false;

    public SecretPhraseView()
    {
        InitializeComponent();
        CreateBorders();

        SlidePanel.RenderTransform = new TranslateTransform(0, SlidePanel.Height);

        // Hook up event handler
        ContinueButton.Click += (s, e) => AnimateSlidePanel(true);
        MainPanel.PointerReleased += (s, e) => AnimateSlidePanel(false);
    }

    private async void FinishClick(object sender, RoutedEventArgs e)
    {
        await MainWindow.SwitchView(new MainWalletView());
    }

    private void OnCheckboxChanged(object? sender, RoutedEventArgs e)
    {
        // Enable the button only if all checkboxes are checked
        SubmitButton.IsEnabled = CheckBox1.IsChecked == true && CheckBox2.IsChecked == true && CheckBox3.IsChecked == true;
    }

    private void CreateBorders()
    {
        int wordIndex = 0;

        // Loop through the text items and create a Border for each
        foreach (var word in Mnemonic.MnemonicWords)
        {
            wordIndex++;

            var stackpanel = new StackPanel
            {
                Width = 80,
                Height = 50,
                Spacing = 5,
                Background = Brushes.Transparent
            };

            var mnemonicWord = new TextBlock
            {
                Text = word,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse("#3E2723")),
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                Effect = new BlurEffect { Radius = 5 },
                Transitions = new Transitions
                {
                    new EffectTransition
                    {
                        Property = TextBlock.EffectProperty,
                        Duration = TimeSpan.FromMilliseconds(300)
                    }
                }
            };

            var separator = new Border
            {
                Width = 85,
                Height = 1,
                Background = new SolidColorBrush(Color.Parse("#5D4037")),
                CornerRadius = new CornerRadius(100),
                BoxShadow = new BoxShadows(
                    new BoxShadow
                    {
                        OffsetX = 5,
                        OffsetY = 5,
                        Blur = 10,
                        Spread = 0,
                        Color = Colors.Black
                    })
            };

            var mnemonicIndex = new TextBlock
            {
                Text = wordIndex.ToString(),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse("#3E2723")),
                FontSize = 12,
                FontWeight = FontWeight.Medium
            };


            stackpanel.Children.Add(mnemonicWord);
            stackpanel.Children.Add(separator);
            stackpanel.Children.Add(mnemonicIndex);

            PhraseGrid.Children.Add(stackpanel);

            stackpanel.PointerEntered += (sender, eventArgs) =>
            {
                // Remove the blur effect to make text clear
                mnemonicWord.Effect = null;
            };

            stackpanel.PointerExited += (sender, eventArgs) =>
            {
                // Add blur effect back to hide the text
                mnemonicWord.Effect = new BlurEffect { Radius = 5 };
            };
        }
    }

    private void AnimateSlidePanel(bool toggle)
    {
        if ((toggle && _isPanelVisible) || (!toggle && !_isPanelVisible))
            return;

        // Create animation
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.3),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseOut()
        };

        // Add keyframe for Y position
        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(1.0),
            Setters =
                {
                    new Setter
                    {
                        Property = TranslateTransform.YProperty,
                        Value = toggle ? 0.0 : SlidePanel.Height
                    }
                }
        });

        animation.RunAsync(SlidePanel);
        _isPanelVisible = toggle;
    }
}