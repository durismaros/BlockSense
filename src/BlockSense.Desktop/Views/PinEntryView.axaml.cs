using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

/// <summary>
/// View that allows the user to choose a 6-digit PIN for a new wallet.
/// After the first entry a confirmation panel slides in to verify the choice.
/// </summary>
public partial class PinEntryView : UserControl
{
    private const int PinLength = 6;

    private static readonly IBrush EmptyDotBackground = new SolidColorBrush(Color.Parse("#F8F1E5"));
    private static readonly IBrush EmptyDotBorderBrush = new SolidColorBrush(Color.Parse("#C4A484"));
    private static readonly IBrush FilledDotBackground = new SolidColorBrush(Color.Parse("#6F4E37"));
    private static readonly DropShadowEffect FilledDotShadow = new()
    {
        BlurRadius = 4,
        Opacity = 0.5,
        Color = Color.Parse("#614E3E")
    };

    private readonly IWalletService _walletService;
    private readonly NavigationManager _navigationManager;
    private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;
    private readonly ILogger<PinEntryView> _logger;

    private readonly Border[] _pinDots = new Border[PinLength];

    private string _currentPin = string.Empty;

    /// <summary>
    /// Gets or sets the mnemonic phrase to protect with the PIN being chosen.
    /// Cleared immediately after the wallet is saved.
    /// </summary>
    public static NBitcoin.Mnemonic? Mnemonic { get; set; }

    /// <summary>
    /// Initialises a new instance of <see cref="PinEntryView"/>.
    /// </summary>
    public PinEntryView()
    {
        _walletService = App.ServiceProvider.GetRequiredService<IWalletService>()
            ?? throw new ArgumentNullException(nameof(IWalletService));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _pinEntrySlidingPanel = MainWindow.Instance.PinEntrySlidingPanel
            ?? throw new ArgumentNullException(nameof(PinEntrySlidingPanel));

        _logger = App.ServiceProvider.GetRequiredService<ILogger<PinEntryView>>()
            ?? throw new ArgumentNullException(nameof(ILogger<PinEntryView>));

        InitializeComponent();
        BuildPinDots();

        Focusable = true;

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _currentPin = string.Empty;
        Focus();

        KeyDown += OnKeyPressed;
        HomeButton.Click += OnHomeButtonClicked;
        ConfirmPinButton.Click += OnConfirmPinButtonClicked;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _currentPin = string.Empty;

        KeyDown -= OnKeyPressed;
        HomeButton.Click -= OnHomeButtonClicked;
        ConfirmPinButton.Click -= OnConfirmPinButtonClicked;
    }

    /// <summary>
    /// Navigates back to the wallet selection view.
    /// </summary>
    private async void OnHomeButtonClicked(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WalletSelectionView>();
    }

    /// <summary>
    /// Opens the confirmation sliding panel when a full PIN has been entered.
    /// </summary>
    private void OnConfirmPinButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (_currentPin.Length != PinLength)
            return;

        var chosenPin = _currentPin;

        _pinEntrySlidingPanel.ShowPanel(async confirmedPin =>
        {
            if (confirmedPin != chosenPin)
            {
                await _pinEntrySlidingPanel.ShowErrorState();
                return;
            }

            await SaveWalletAndContinueAsync(chosenPin);
        });
    }

    /// <summary>
    /// Handles keyboard input: digit keys fill dots, Backspace clears, Escape navigates back.
    /// </summary>
    private async void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        char? digit = TryGetDigitChar(e.Key);

        if (digit.HasValue && _currentPin.Length < PinLength)
        {
            _currentPin += digit.Value;
            await AnimateDotFilledAsync(_currentPin.Length - 1);

            if (_currentPin.Length == PinLength)
                ConfirmPinButton.IsEnabled = true;
        }
        else if (e.Key == Key.Back && _currentPin.Length > 0)
        {
            int lastIndex = _currentPin.Length - 1;
            _currentPin = _currentPin[..lastIndex];

            await AnimateDotClearedAsync(lastIndex);
            ConfirmPinButton.IsEnabled = false;
        }
        else if (e.Key == Key.Escape)
        {
            await _navigationManager.NavigateToAsync<WalletSelectionView>();
        }
    }

    private async Task SaveWalletAndContinueAsync(string pin)
    {
        try
        {
            if (Mnemonic is null || !Mnemonic.IsValidChecksum)
                throw new InvalidOperationException("Invalid or missing mnemonic.");

            _logger.LogInformation("Creating wallet with provided mnemonic and PIN.");
            await _walletService.CreateWalletAsync(Mnemonic, pin);

            _pinEntrySlidingPanel.HidePanel();
            await _navigationManager.NavigateToAsync<CryptoWalletView>();
        }
        finally
        {
            Mnemonic = null;
        }
    }

    private static char? TryGetDigitChar(Key key)
    {
        if (key >= Key.D0 && key <= Key.D9)
            return (char)('0' + (key - Key.D0));

        return null;
    }

    private void BuildPinDots()
    {
        for (int i = 0; i < PinLength; i++)
        {
            _pinDots[i] = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(10),
                Background = EmptyDotBackground,
                BorderThickness = new Thickness(1),
                BorderBrush = EmptyDotBorderBrush
            };

            DotsPanel.Children.Add(_pinDots[i]);
        }
    }

    private async Task AnimateDotFilledAsync(int index)
    {
        if (index < 0 || index >= _pinDots.Length)
            return;

        var dot = _pinDots[index];

        dot.Background = FilledDotBackground;
        dot.BorderThickness = new Thickness(0);
        dot.Effect = FilledDotShadow;
        dot.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        dot.RenderTransform = new ScaleTransform();

        await new Animation
        {
            Duration = TimeSpan.FromMilliseconds(150),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue     = new Cue(1.0),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1.3),
                        new Setter(ScaleTransform.ScaleYProperty, 1.3)
                    }
                }
            }
        }.RunAsync(dot);

        await new Animation
        {
            Duration = TimeSpan.FromMilliseconds(150),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseIn(),
            Children =
            {
                new KeyFrame
                {
                    Cue     = new Cue(1.0),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0)
                    }
                }
            }
        }.RunAsync(dot);
    }

    private async Task AnimateDotClearedAsync(int index)
    {
        if (index < 0 || index >= _pinDots.Length)
            return;

        var dot = _pinDots[index];

        dot.Background = EmptyDotBackground;
        dot.BorderThickness = new Thickness(1);
        dot.BorderBrush = EmptyDotBorderBrush;
        dot.Effect = null;
        dot.RenderTransform = new ScaleTransform(1, 1);

        await new Animation
        {
            Duration = TimeSpan.FromMilliseconds(100),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame { Cue = new Cue(0.0), Setters = { new Setter(OpacityProperty, 0.5) } },
                new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(OpacityProperty, 1.0) } }
            }
        }.RunAsync(dot);
    }
}
