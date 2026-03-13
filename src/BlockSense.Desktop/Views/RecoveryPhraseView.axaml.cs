using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Implementations;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBitcoin;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

/// <summary>
/// View that displays the newly generated 12-word BIP-39 recovery phrase,
/// blurred by default with hover-to-reveal per word.
/// </summary>
public partial class RecoveryPhraseView : UserControl
{
    private readonly ICurrentWalletProvider _currentWalletProvider;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<RecoveryPhraseView> _logger;

    private readonly string _mnemonic;

    /// <summary>
    /// Initialises a new instance of <see cref="RecoveryPhraseView"/> and
    /// generates a new mnemonic phrase.
    /// </summary>
    public RecoveryPhraseView()
    {
        _currentWalletProvider = App.ServiceProvider.GetRequiredService<ICurrentWalletProvider>()
            ?? throw new ArgumentNullException(nameof(ICurrentWalletProvider));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _logger = App.ServiceProvider.GetRequiredService<ILogger<RecoveryPhraseView>>()
            ?? throw new ArgumentNullException(nameof(ILogger<RecoveryPhraseView>));

        _mnemonic = WalletService.GenerateMnemonic();

        InitializeComponent();
        BuildPhraseWordGrid(_mnemonic);

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        SlidePanel.RenderTransform = new TranslateTransform(0, SlidePanel.Height);

        HomeButton.Click += OnHomeButtonClicked;
        ContinueButton.Click += OnContinueButtonClicked;
        ContentGrid.PointerPressed += OnContentGridPointerPressed;
        SubmitButton.Click += OnSubmitButtonClicked;
        CheckBox1.IsCheckedChanged += OnAcknowledgementCheckBoxChanged;
        CheckBox2.IsCheckedChanged += OnAcknowledgementCheckBoxChanged;
        CheckBox3.IsCheckedChanged += OnAcknowledgementCheckBoxChanged;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        HomeButton.Click -= OnHomeButtonClicked;
        ContinueButton.Click -= OnContinueButtonClicked;
        ContentGrid.PointerPressed -= OnContentGridPointerPressed;
        SubmitButton.Click -= OnSubmitButtonClicked;
        CheckBox1.IsCheckedChanged -= OnAcknowledgementCheckBoxChanged;
        CheckBox2.IsCheckedChanged -= OnAcknowledgementCheckBoxChanged;
        CheckBox3.IsCheckedChanged -= OnAcknowledgementCheckBoxChanged;
    }

    /// <summary>
    /// Navigates back to the wallet selection view.
    /// </summary>
    private async void OnHomeButtonClicked(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WalletSelectionView>();
    }

    private void OnContinueButtonClicked(object? sender, RoutedEventArgs e)
        => _ = AnimateSlidePanelAsync(open: true);

    private void OnContentGridPointerPressed(object? sender, PointerPressedEventArgs e)
        => _ = AnimateSlidePanelAsync(open: false);

    /// <summary>
    /// Sets the mnemonic on <see cref="PinEntryView"/> and navigates to PIN entry.
    /// </summary>
    private async void OnSubmitButtonClicked(object? sender, RoutedEventArgs e)
    {
        PinEntryView.Mnemonic = new Mnemonic(_mnemonic, Wordlist.English);
        _logger.LogInformation("Recovery phrase acknowledged — navigating to PIN entry.");
        await _navigationManager.NavigateToAsync<PinEntryView>();
    }

    /// <summary>
    /// Enables or disables the submit button based on the three acknowledgement checkboxes.
    /// </summary>
    private void OnAcknowledgementCheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        SubmitButton.IsEnabled =
            CheckBox1.IsChecked == true &&
            CheckBox2.IsChecked == true &&
            CheckBox3.IsChecked == true;
    }

    private void BuildPhraseWordGrid(string mnemonic)
    {
        PhraseGrid.Children.Clear();

        int wordNumber = 0;

        foreach (var word in mnemonic.Split(' '))
        {
            wordNumber++;

            var wordLabel = new TextBlock
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

            var separator = BuildWordSeparator();

            var indexLabel = new TextBlock
            {
                Text = wordNumber.ToString(),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse("#3E2723")),
                FontSize = 12,
                FontWeight = FontWeight.Medium
            };

            var wordStack = new StackPanel
            {
                Width = 80,
                Height = 50,
                Spacing = 5,
                Background = Brushes.Transparent
            };

            wordStack.Children.Add(wordLabel);
            wordStack.Children.Add(separator);
            wordStack.Children.Add(indexLabel);

            wordStack.PointerEntered += (_, _) => wordLabel.Effect = null;
            wordStack.PointerExited += (_, _) => wordLabel.Effect = new BlurEffect { Radius = 5 };

            PhraseGrid.Children.Add(wordStack);
        }
    }

    private async Task AnimateSlidePanelAsync(bool open)
    {
        var targetY = open ? 0.0 : SlidePanel.Height;

        await new Animation
        {
            Duration = TimeSpan.FromSeconds(0.3),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue     = new Cue(1.0),
                    Setters = { new Setter(TranslateTransform.YProperty, targetY) }
                }
            }
        }.RunAsync(SlidePanel);
    }

    private static Border BuildWordSeparator() => new()
    {
        Width = 85,
        Height = 1,
        Background = new SolidColorBrush(Color.Parse("#5D4037")),
        CornerRadius = new CornerRadius(100),
        BoxShadow = new Avalonia.Media.BoxShadows(new Avalonia.Media.BoxShadow
        {
            OffsetX = 5,
            OffsetY = 5,
            Blur = 10,
            Spread = 0,
            Color = Avalonia.Media.Colors.Black
        })
    };
}
