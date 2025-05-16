using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using System.Threading.Tasks;
using System;
using BlockSense.Views;
using System.Collections.Generic;
using BlockSense.Cryptography.Wallet.MnemonicManager;

namespace BlockSense;

public partial class PinEntryView : UserControl
{
    private const int PIN_LENGTH = 6;
    private string _currentPin = string.Empty;
    private string _confirmPin = string.Empty;
    private bool _isPanelVisible = false;

    private readonly Border[] _currentDots = new Border[PIN_LENGTH];
    private readonly Border[] _confirmDots = new Border[PIN_LENGTH];

    private static readonly IBrush EmptyDotBackground = new SolidColorBrush(Color.Parse("#F8F1E5"));
    private static readonly IBrush EmptyDotBorder = new SolidColorBrush(Color.Parse("#C4A484"));
    private static readonly IBrush FilledDotBackground = new SolidColorBrush(Color.Parse("#6F4E37"));
    private static readonly DropShadowEffect FilledDotShadow = new()
    {
        BlurRadius = 4,
        Opacity = 0.5,
        Color = Color.Parse("#614E3E")
    };

    public bool SetupMode { get; set; }

    public PinEntryView()
    {
        InitializeComponent();
        SetMode(true); // set initial mode Here

        // Initialize sliding panel position
        SlidePanel.RenderTransform = new TranslateTransform(0, -450);

        SetupDots(DotsPanel, _currentDots);
        SetupDots(ConfirmDotsPanel, _confirmDots);


        // Event subscribing

        // Back from Confirming PIN
        BackButton.Click += (s, e) =>
        {
            _confirmPin = string.Empty;
            ResetDots(_confirmDots);
            AnimatePanel(false);
            this.Focus();
        };

        EnterPinButton.Click += (s, e) =>
        {
            if (_currentPin.Length == PIN_LENGTH && SetupMode)
            {
                ConfirmPinButton.IsEnabled = false;
                ConfirmPinButton.Opacity = 0.5;
                AnimatePanel(true);
                this.Focus();
            }
            else if (_currentPin.Length == PIN_LENGTH && !SetupMode)
            {
                ValidateAndCompleteEntry();
            }
        };

        ConfirmPinButton.Click += (s, e) =>
        {
            if (_confirmPin.Length == PIN_LENGTH)
            {
                ValidateAndCompleteSetup();
                this.Focus();
            }
        };

        this.Focusable = true;
        this.KeyDown += OnKeyDown;
        this.Loaded += (s, e) => this.Focus();
    }

    private void DragWindow(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && VisualRoot is Window window)
            window.BeginMoveDrag(e);
    }

    private async void AnimatePanel(bool show)
    {
        if ((show && _isPanelVisible) || (!show && !_isPanelVisible)) return;

        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.3),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseOut(),
            Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(1.0),
                        Setters = { new Setter(TranslateTransform.YProperty, show ? 0.0 : -SlidePanel.Bounds.Height) }
                    }
                }
        };

        await animation.RunAsync(SlidePanel);
        _isPanelVisible = show;
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Handle digit keys - determine which PIN we're editing
        bool confirmMode = _isPanelVisible;
        ref string pin = ref (confirmMode ? ref _confirmPin : ref _currentPin);
        Border[] dots = confirmMode ? _confirmDots : _currentDots;
        Button button = confirmMode ? ConfirmPinButton : EnterPinButton;

        // Handle Numeric keys
        if ((e.Key >= Key.D0 && e.Key <= Key.D9) && pin.Length < PIN_LENGTH)
        {
            // Update the appropriate PIN
            pin += (e.Key - Key.D0).ToString();
            AnimateDotFill(dots, pin.Length - 1);
            if (pin.Length.Equals(PIN_LENGTH))
            {
                button.IsEnabled = true;
                button.Opacity = 1.0;
            }
        }

        // Handle backspace key
        else if (e.Key.Equals(Key.Back) && pin.Length > 0)
        {
            pin = pin.Substring(0, pin.Length - 1);
            AnimateDotClear(dots, pin.Length);
            button.IsEnabled = false;
            button.Opacity = 0.5;
        }

        // Handle Enter key
        else if (e.Key.Equals(Key.Enter) && pin.Length.Equals(PIN_LENGTH))
        {
            if (confirmMode) ValidateAndCompleteSetup();
            else
            {
                if (SetupMode)
                {
                    ConfirmPinButton.IsEnabled = false;
                    ConfirmPinButton.Opacity = 0.5;
                    AnimatePanel(true);
                }
                else
                {
                    ValidateAndCompleteEntry();
                }
            }
        }

        // Handle Escape key
        else if (e.Key.Equals(Key.Escape) && !_isPanelVisible)
        {
            //await MainWindow.SwitchView(new WelcomeView());
            ResetPin();
        }
    }

    private void ValidateAndCompleteEntry()
    {
        //List<string> retrievedMnemonic = MnemonicManager.RetrieveMnemonic(_currentPin);
    }

    private async void ValidateAndCompleteSetup()
    {
        if (_currentPin.Equals(_confirmPin))
        {
            MnemonicManager.StoreMnemonic(MnemonicManager.MnemonicWords, _confirmPin);
            SetupMode = false;
            AnimatePanel(false);
            ResetPin();
            //await MainWindow.SwitchView(new SecretPhraseView());
            // Wallet generation logic here
        }
        else
        {
            // PIN mismatch - shake the confirmation panel and reset the confirm PIN
            _confirmPin = string.Empty;
            await ShakePanel();
            ResetDots(_confirmDots);
            ConfirmPinButton.IsEnabled = false;
            ConfirmPinButton.Opacity = 0.5;
        }
    }

    private void SetupDots(StackPanel panel, Border[] dots)
    {
        for (int i = 0; i < PIN_LENGTH; i++)
        {
            dots[i] = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(10),
                Background = EmptyDotBackground,
                BorderThickness = new Thickness(1),
                BorderBrush = EmptyDotBorder
            };
            panel.Children.Add(dots[i]);
        }
    }

    private void ResetDots(Border[] dots)
    {
        for (int i = 0; i < dots.Length; i++)
            AnimateDotClear(dots, i);
    }

    private async void AnimateDotFill(Border[] dots, int index)
    {
        if (index < 0 || index >= dots.Length) return;

        var dot = dots[index];
        var transform = new ScaleTransform();
        dot.RenderTransform = transform;
        dot.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

        // Scale up animation
        for (double scale = 1.0; scale <= 1.3; scale += 0.05)
        {
            transform.ScaleX = transform.ScaleY = scale;
            await Task.Delay(8);
        }

        // Change color to filled state
        dot.Background = FilledDotBackground;
        dot.BorderThickness = new Thickness(0);
        dot.Effect = FilledDotShadow;

        // Scale down animation
        for (double scale = 1.3; scale >= 1.0; scale -= 0.05)
        {
            transform.ScaleX = transform.ScaleY = scale;
            await Task.Delay(8);
        }
    }

    private void AnimateDotClear(Border[] dots, int index)
    {
        if (index < 0 || index >= dots.Length) return;

        var dot = dots[index];
        dot.Background = EmptyDotBackground;
        dot.BorderThickness = new Thickness(1);
        dot.BorderBrush = EmptyDotBorder;
        dot.Effect = null;

        dot.RenderTransform = new ScaleTransform(1, 1); // Reset any transforms
    }

    private async Task ShakePanel()
    {
        // Create a transform for the entire ConfirmDotsPanel
        var translatePosition = new TranslateTransform();
        ConfirmDotsPanel.RenderTransform = translatePosition;

        // Shake animation offsets
        double[] shakeOffsets = { 0, -10, 10, -6, 6, -3, 3, 0 };

        // Execute shake animation
        for (int i = 0; i < shakeOffsets.Length; i++)
        {
            translatePosition.X = shakeOffsets[i];
            await Task.Delay(20);
        }

        // Reset to original state
        ConfirmDotsPanel.RenderTransform = new TranslateTransform(0, 0);
    }

    // Public method to reset the PIN entry (for use by parent controls)
    public void ResetPin()
    {
        _currentPin = string.Empty;
        _confirmPin = string.Empty;
        ResetDots(_currentDots);
        ResetDots(_confirmDots);
        EnterPinButton.IsEnabled = false;
        EnterPinButton.Opacity = 0.5;
        ConfirmPinButton.IsEnabled = false;
        ConfirmPinButton.Opacity = 0.5;

        if (_isPanelVisible)
            AnimatePanel(false);

        this.Focus();
    }

    // Method to switch between setup mode and verification mode
    public void SetMode(bool setupMode)
    {
        SetupMode = setupMode;
        InitialPinPromt.Text = setupMode ? "Create PIN" : "Enter Your PIN";
    }
}