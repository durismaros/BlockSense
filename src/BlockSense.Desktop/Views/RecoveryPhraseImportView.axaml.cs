using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockSense.Desktop;

public partial class RecoveryPhraseImportView : UserControl
{
    private readonly IWalletProvider _walletProvider;
    private readonly NavigationManager _navigationManager;
    private readonly List<TextBox> _wordInputs = new();

    private static readonly HashSet<string> _bip39Words =
        Wordlist.English.GetWords().ToHashSet(StringComparer.OrdinalIgnoreCase);

    public RecoveryPhraseImportView()
    {
        _walletProvider = App.ServiceProvider.GetRequiredService<IWalletProvider>()
            ?? throw new ArgumentNullException(nameof(IWalletProvider));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        InitializeComponent();
        CreateWordInputs();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private async void ToWalletSelectionViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WalletSelectionView>();
    }

    private async void ToPinEntryViewClick(object? sender, RoutedEventArgs e)
    {
        _walletProvider.SetCreationContext(string.Join(" ", GetMnemonicWords()), isImport: true);

        await _navigationManager.NavigateToAsync<PinEntryView>();
    }

    private void CreateWordInputs()
    {
        PhraseGrid.Children.Clear();
        _wordInputs.Clear();

        for (int i = 1; i <= 12; i++)
        {
            int wordIndex = i; // capture for closure

            var stackPanel = new StackPanel
            {
                Width = 80,
                Height = 50,
                Spacing = 5,
                Background = Brushes.Transparent
            };

            var mnemonicTextBox = new TextBox
            {
                Watermark = "enter",
                BorderThickness = new Thickness(0),
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
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

            stackPanel.Children.Add(mnemonicTextBox);
            stackPanel.Children.Add(separator);
            stackPanel.Children.Add(mnemonicIndex);

            PhraseGrid.Children.Add(stackPanel);
            _wordInputs.Add(mnemonicTextBox);
        }
    }

    private async void AnimateSlidePanel(bool toggle)
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.3),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseOut()
        };

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

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        SlidePanel.RenderTransform = new TranslateTransform(0, SlidePanel.Height);

        HomeButton.Click += ToWalletSelectionViewClick;
        ContinueButton.Click += (s, e) => AnimateSlidePanel(true);
        ContentGrid.PointerPressed += (s, ev) => AnimateSlidePanel(false);
        SubmitButton.Click += ToPinEntryViewClick;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        HomeButton.Click -= ToWalletSelectionViewClick;
        ContinueButton.Click -= (s, e) => AnimateSlidePanel(true);
        ContentGrid.PointerPressed -= (s, ev) => AnimateSlidePanel(false);
        SubmitButton.Click -= ToPinEntryViewClick;
    }

    public IReadOnlyList<string> GetMnemonicWords()
        => _wordInputs.Select(tb => tb.Text?.Trim() ?? string.Empty).ToList();
}