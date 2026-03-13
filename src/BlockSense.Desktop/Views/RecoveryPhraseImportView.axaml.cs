using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

/// <summary>
/// View that allows the user to restore a wallet by entering their 12-word BIP-39 recovery phrase.
/// </summary>
public partial class RecoveryPhraseImportView : UserControl
{
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<RecoveryPhraseImportView> _logger;
    private readonly List<TextBox> _wordInputs = new();

    /// <summary>
    /// Initialises a new instance of <see cref="RecoveryPhraseImportView"/>.
    /// </summary>
    public RecoveryPhraseImportView()
    {
        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _logger = App.ServiceProvider.GetRequiredService<ILogger<RecoveryPhraseImportView>>()
            ?? throw new ArgumentNullException(nameof(ILogger<RecoveryPhraseImportView>));

        InitializeComponent();
        BuildWordInputs();

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
    /// Validates the recovery phrase and navigates to PIN entry.
    /// </summary>
    private async void OnSubmitButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (!TrySetValidMnemonic())
            return;

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

    /// <summary>
    /// Validates the entered recovery phrase words and sets
    /// <see cref="PinEntryView.Mnemonic"/> if valid.
    /// </summary>
    /// <returns><see langword="true"/> if a valid mnemonic was set.</returns>
    public bool TrySetValidMnemonic()
    {
        if (_wordInputs.Any(input => string.IsNullOrWhiteSpace(input.Text)))
        {
            MainWindow.Instance.ShowNotification(
                "Invalid Phrase",
                "Please fill in all mnemonic words.");
            return false;
        }

        var phrase = string.Join(" ", _wordInputs.Select(input => input.Text?.Trim()));
        var mnemonic = new Mnemonic(phrase, Wordlist.English);

        if (!mnemonic.IsValidChecksum)
        {
            _logger.LogWarning("User entered an invalid mnemonic phrase.");
            MainWindow.Instance.ShowNotification(
                "Invalid Phrase",
                "Entered mnemonic phrase seems to be invalid.");
            return false;
        }

        PinEntryView.Mnemonic = mnemonic;
        _logger.LogInformation("Valid mnemonic phrase accepted.");
        return true;
    }

    private void BuildWordInputs()
    {
        PhraseGrid.Children.Clear();
        _wordInputs.Clear();

        for (int wordNumber = 1; wordNumber <= 12; wordNumber++)
        {
            var inputBox = new TextBox
            {
                Classes = { "NoUnderline" },
                Watermark = "enter",
                BorderThickness = new Thickness(0),
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var separator = BuildWordSeparator();

            var indexLabel = new TextBlock
            {
                Text = wordNumber.ToString(),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse("#3E2723")),
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.Medium
            };

            var wordStack = new StackPanel
            {
                Width = 110,
                Height = 50,
                Spacing = 5,
                Background = Avalonia.Media.Brushes.Transparent
            };

            wordStack.Children.Add(inputBox);
            wordStack.Children.Add(separator);
            wordStack.Children.Add(indexLabel);

            PhraseGrid.Children.Add(wordStack);
            _wordInputs.Add(inputBox);
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
        Width = 100,
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
