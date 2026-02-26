using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class PinEntrySlidingPanel : UserControl
{
    private const int PIN_LENGTH = 6;

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

    public Func<string, Task>? OnSubmitAsync
    {
        get;
        private set;
    }

    public PinEntrySlidingPanel()
    {
        InitializeComponent();
        SetupPinDots();

        this.Focusable = true;

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    public async void ShowPanel(Func<string, Task> onSubmitAsync)
    {
        if (this.IsVisible)
        {
            return;
        }

        // Initialize sliding panel position
        this.RenderTransform = new TranslateTransform(0, -MainWindow.Instance.Height);

        OnSubmitAsync = onSubmitAsync
            ?? throw new ArgumentNullException(nameof(onSubmitAsync));

        IsVisible = true;

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
                    Setters = { new Setter(TranslateTransform.YProperty, 0.0) }
                }
            }
        }.RunAsync(this);

        await ResetPinEntry();

        await animation;
    }

    public async void HidePanel(object? sender = default, RoutedEventArgs? e = default)
    {
        if (!this.IsVisible)
        {
            return;
        }

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
                    Setters = { new Setter(TranslateTransform.YProperty, -this.Bounds.Height) }
                }
            }
        };

        await animation.RunAsync(this);

        this.IsVisible = false;
    }

    public async Task ShowErrorState()
    {
        await Task.WhenAll(
            ShakePanel(),
            ResetPinEntry()
            );
    }

    private async void VerifyPinClick(object? sender, RoutedEventArgs e)
    {
        if (_currentPin.Length != PIN_LENGTH)
        {
            return;
        }

        if (OnSubmitAsync is null)
        {
            return;
        }

        await OnSubmitAsync(_currentPin);
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        char? inputChar = GetInputChar(e.Key);

        if (inputChar.HasValue && _currentPin.Length < PIN_LENGTH)
        {
            _currentPin += inputChar.Value;
            await AnimatePinEntry(_currentPin.Length - 1);

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

            await AnimatePinClear(lastIndex);

            ConfirmPinButton.IsEnabled = false;
        }

        else if (e.Key == Key.Escape)
        {
            //
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

            ConfirmDotsPanel.Children.Add(_currentDots[i]);
        }
    }

    private async Task ResetPinEntry()
    {
        _currentPin = string.Empty;

        for (int i = 0; i < _currentDots.Length; i++)
        {
            await AnimatePinClear(i);
        }

        ConfirmPinButton.IsEnabled = false;

        this.Focus();
    }

    private async Task AnimatePinEntry(int index)
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

    private async Task AnimatePinClear(int index)
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

    private void DragWindow(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && VisualRoot is Window window)
            window.BeginMoveDrag(e);
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        this.Focus();

        this.KeyDown += OnKeyDown;
        DragBorder.PointerPressed += DragWindow;
        BackButton.Click += HidePanel;
        ConfirmPinButton.Click += VerifyPinClick;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        this.KeyDown -= OnKeyDown;
        DragBorder.PointerPressed -= DragWindow;
        BackButton.Click -= HidePanel;
        ConfirmPinButton.Click -= VerifyPinClick;
    }
}