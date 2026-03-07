using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class PinEntryView : UserControl
{
    private const int PIN_LENGTH = 6;

    private readonly IWalletService _walletService;
    private readonly NavigationManager _navigationManager;
    private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;

    private readonly Border[] _currentDots = new Border[PIN_LENGTH];

    private static readonly IBrush EmptyDotBackground = new SolidColorBrush(Color.Parse("#F8F1E5"));
    private static readonly IBrush EmptyDotBorder = new SolidColorBrush(Color.Parse("#C4A484"));
    private static readonly IBrush FilledDotBackground = new SolidColorBrush(Color.Parse("#6F4E37"));
    private static readonly DropShadowEffect FilledDotShadow = new()
    {
        BlurRadius = 4,
        Opacity = 0.5,
        Color = Color.Parse("#614E3E")
    };

    private string _currentPin = string.Empty;

    public PinEntryView()
    {
        _walletService = App.ServiceProvider.GetRequiredService<IWalletService>()
            ?? throw new ArgumentNullException(nameof(IWalletService));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _pinEntrySlidingPanel = MainWindow.Instance.PinEntrySlidingPanel
            ?? throw new ArgumentNullException(nameof(PinEntrySlidingPanel));

        InitializeComponent();
        SetupPinDots();

        this.Focusable = true;

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private async void ToWalletSelectionViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WalletSelectionView>();
    }

    private async void VerifyPinClick(object? sender, RoutedEventArgs e)
    {
        if (_currentPin.Length != PIN_LENGTH) return;

        _pinEntrySlidingPanel.ShowPanel(async confirmPin =>
        {
            if (confirmPin != _currentPin)
            {
                await _pinEntrySlidingPanel.ShowErrorState();
                return;
            }

            // PINs match — save wallet and proceed
            await SaveWalletAndContinueAsync(_currentPin);
        });
    }

    private async Task SaveWalletAndContinueAsync(string pin)
    {
        try
        {
            var pendingContext = _walletProvider.CreationContext
                ?? throw new ArgumentNullException(nameof(WalletCreationContext));

            var wallet = pendingContext.IsImport
                ? await _walletService.ImportWalletAsync(pendingContext.Mnemonic, pin)
                : await _walletService.CreateWalletAsync(pendingContext.Mnemonic, pin);

            _walletProvider.SetWallet(wallet);
            _walletProvider.ClearCreationContext();

            _pinEntrySlidingPanel.HidePanel();

            await _navigationManager.NavigateToAsync<CryptoWalletView>();
        }
        catch
        {
            _pinEntrySlidingPanel.HidePanel();
            await _navigationManager.NavigateToAsync<WalletSelectionView>();
        }
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        char? inputChar = GetInputChar(e.Key);

        if (inputChar.HasValue && _currentPin.Length < PIN_LENGTH)
        {
            _currentPin += inputChar.Value;
            await AnimateDotFill(_currentPin.Length - 1);

            if (_currentPin.Length == PIN_LENGTH)
            {
                ConfirmPinButton.IsEnabled = true;
            }
        }

        // Handle Backspace
        else if (e.Key == Key.Back && _currentPin.Length > 0)
        {
            int lastIndex = _currentPin.Length - 1;
            _currentPin = _currentPin.Substring(0, lastIndex);

            await AnimateDotClear(lastIndex);

            ConfirmPinButton.IsEnabled = false;
        }

        else if (e.Key == Key.Escape)
        {
            await _navigationManager.NavigateToAsync<WalletSelectionView>();
        }
    }

    private char? GetInputChar(Key key)
    {
        if (key >= Key.D0 && key <= Key.D9)
        {
            return (char)('0' + (key - Key.D0));
        }

        return null;
    }

    private void SetupPinDots()
    {
        for (int i = 0; i < PIN_LENGTH; i++)
        {
            _currentDots[i] = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(10),
                Background = EmptyDotBackground,
                BorderThickness = new Thickness(1),
                BorderBrush = EmptyDotBorder
            };

            DotsPanel.Children.Add(_currentDots[i]);
        }
    }

    private async Task AnimateDotFill(int index)
    {
        if (index < 0 || index >= _currentDots.Length)
        {
            return;
        }

        var dot = _currentDots[index];

        dot.Background = FilledDotBackground;
        dot.BorderThickness = new Thickness(0);
        dot.Effect = FilledDotShadow;

        var transform = new ScaleTransform();
        dot.RenderTransform = transform;
        dot.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

        var scaleUp = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(150),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1.3),
                        new Setter(ScaleTransform.ScaleYProperty, 1.3)
                    }
                }
            }
        };
        await scaleUp.RunAsync(dot);

        var scaleDown = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(150),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseIn(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0)
                    }
                }
            }
        };
        await scaleDown.RunAsync(dot);
    }

    private async Task AnimateDotClear(int index)
    {
        if (index < 0 || index >= _currentDots.Length)
        {
            return;
        }

        var dot = _currentDots[index];

        dot.Background = EmptyDotBackground;
        dot.BorderThickness = new Thickness(1);
        dot.BorderBrush = EmptyDotBorder;
        dot.Effect = null;
        dot.RenderTransform = new ScaleTransform(1, 1);

        var fade = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(100),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Setter(OpacityProperty, 0.5) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Setter(OpacityProperty, 1.0) }
                }
            }
        };
        await fade.RunAsync(dot);
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        this.Focus();

        _currentPin = string.Empty;

        this.KeyDown += OnKeyDown;
        HomeButton.Click += ToWalletSelectionViewClick;
        ConfirmPinButton.Click += VerifyPinClick;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _currentPin = string.Empty;

        this.KeyDown -= OnKeyDown;
        HomeButton.Click -= ToWalletSelectionViewClick;
        ConfirmPinButton.Click -= VerifyPinClick;
    }
}