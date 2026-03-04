using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Implementations;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlockSense.Desktop;

public partial class RecoveryPhraseView : UserControl
{
    private readonly IWalletProvider _walletProvider;
    private readonly NavigationManager _navigationManager;
    private readonly string _mnemonic;

    public RecoveryPhraseView()
    {
        _walletProvider = App.ServiceProvider.GetRequiredService<IWalletProvider>()
            ?? throw new ArgumentNullException(nameof(IWalletProvider));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _mnemonic = WalletService.GenerateMnemonic();

        InitializeComponent();
        CreateBorders(_mnemonic);

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private async void ToWalletSelectionViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WalletSelectionView>();
    }

    private async void ToPinEntryViewClick(object? sender, RoutedEventArgs e)
    {
        _walletProvider.SetCreationContext(_mnemonic, isImport: false);

        await _navigationManager.NavigateToAsync<PinEntryView>();
    }

    private void CreateBorders(string mnemonic)
    {
        PhraseGrid.Children.Clear();

        int wordIndex = 0;

        // Loop through the text items and create a Border for each
        foreach (var word in mnemonic.Split(' '))
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
                        Property = EffectProperty,
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
                mnemonicWord.Effect = new BlurEffect
                {
                    Radius = 5
                };
            };
        }
    }

    private async void AnimateSlidePanel(bool toggle)
    {
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

        await animation.RunAsync(SlidePanel);
    }

    private void OnCheckboxChanged(object? sender, RoutedEventArgs e)
    {
        SubmitButton.IsEnabled = CheckBox1.IsChecked == true && CheckBox2.IsChecked == true && CheckBox3.IsChecked == true;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        SlidePanel.RenderTransform = new TranslateTransform(0, SlidePanel.Height);

        HomeButton.Click += ToWalletSelectionViewClick;
        ContinueButton.Click += (s, e) => AnimateSlidePanel(true);
        ContentGrid.PointerPressed += (s, e) => AnimateSlidePanel(false);
        SubmitButton.Click += ToPinEntryViewClick;

        CheckBox1.IsCheckedChanged += OnCheckboxChanged;
        CheckBox2.IsCheckedChanged += OnCheckboxChanged;
        CheckBox3.IsCheckedChanged += OnCheckboxChanged;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        HomeButton.Click -= ToWalletSelectionViewClick;
        ContinueButton.Click -= (s, e) => AnimateSlidePanel(true);
        ContentGrid.PointerPressed -= (s, e) => AnimateSlidePanel(false);
        SubmitButton.Click -= ToPinEntryViewClick;

        CheckBox1.IsCheckedChanged -= OnCheckboxChanged;
        CheckBox2.IsCheckedChanged -= OnCheckboxChanged;
        CheckBox3.IsCheckedChanged -= OnCheckboxChanged;
    }
}