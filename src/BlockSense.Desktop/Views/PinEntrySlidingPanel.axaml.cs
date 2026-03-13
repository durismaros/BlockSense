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

/// <summary>
/// A full-screen sliding panel that prompts the user to re-enter their PIN
/// as a second confirmation step.
/// </summary>
public partial class PinEntrySlidingPanel : UserControl
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

    private readonly Border[] _pinDots = new Border[PinLength];

    private string _currentPin = string.Empty;

    /// <summary>
    /// Gets the async callback invoked when the user submits their PIN.
    /// Set via <see cref="ShowPanel"/>.
    /// </summary>
    public Func<string, Task>? OnSubmitAsync { get; private set; }

    /// <summary>
    /// Initialises a new instance of <see cref="PinEntrySlidingPanel"/>.
    /// </summary>
    public PinEntrySlidingPanel()
    {
        InitializeComponent();
        BuildPinDots();

        Focusable = true;

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Focus();

        KeyDown += OnKeyPressed;
        DragBorder.PointerPressed += OnDragBorderPointerPressed;
        BackButton.Click += HidePanel;
        ConfirmPinButton.Click += OnConfirmPinButtonClicked;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        KeyDown -= OnKeyPressed;
        DragBorder.PointerPressed -= OnDragBorderPointerPressed;
        BackButton.Click -= HidePanel;
        ConfirmPinButton.Click -= OnConfirmPinButtonClicked;
    }

    /// <summary>
    /// Slides the panel into view and stores the callback to invoke on PIN submission.
    /// </summary>
    /// <param name="onSubmitAsync">Callback that receives the entered PIN string.</param>
    public async void ShowPanel(Func<string, Task> onSubmitAsync)
    {
        if (IsVisible)
            return;

        RenderTransform = new TranslateTransform(0, -MainWindow.Instance.Height);

        OnSubmitAsync = onSubmitAsync
            ?? throw new ArgumentNullException(nameof(onSubmitAsync));

        IsVisible = true;
        Focus();

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
                    Setters = { new Setter(TranslateTransform.YProperty, 0.0) }
                }
            }
        }.RunAsync(this);
    }

    /// <summary>
    /// Slides the panel out of view and resets the PIN entry state.
    /// </summary>
    public async void HidePanel(object? sender = default, RoutedEventArgs? e = default)
    {
        if (!IsVisible)
            return;

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
                    Setters = { new Setter(TranslateTransform.YProperty, -Bounds.Height) }
                }
            }
        }.RunAsync(this);

        IsVisible = false;
        await ResetPinEntryAsync();
    }

    /// <summary>
    /// Shakes the PIN dot row and resets the entered PIN to signal an incorrect entry.
    /// </summary>
    public async Task ShowErrorState()
    {
        await Task.WhenAll(ShakePinDotsAsync(), ResetPinEntryAsync());
        Focus();
    }

    private async void OnConfirmPinButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (_currentPin.Length != PinLength || OnSubmitAsync is null)
            return;

        await OnSubmitAsync(_currentPin);
    }

    /// <summary>
    /// Handles keyboard input: digit keys fill the next dot, Backspace clears the last.
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
    }

    /// <summary>
    /// Begins a native window drag when the left mouse button is pressed on the drag area.
    /// </summary>
    private void OnDragBorderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && VisualRoot is Window window)
            window.BeginMoveDrag(e);
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

            ConfirmDotsPanel.Children.Add(_pinDots[i]);
        }
    }

    private async Task ResetPinEntryAsync()
    {
        _currentPin = string.Empty;

        for (int i = 0; i < _pinDots.Length; i++)
            await AnimateDotClearedAsync(i);

        ConfirmPinButton.IsEnabled = false;
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

    private async Task ShakePinDotsAsync()
    {
        var translateTransform = new TranslateTransform();
        ConfirmDotsPanel.RenderTransform = translateTransform;

        double[] offsets = { 0, -10, 10, -6, 6, -3, 3, 0 };

        foreach (var offset in offsets)
        {
            translateTransform.X = offset;
            await Task.Delay(20);
        }

        ConfirmDotsPanel.RenderTransform = new TranslateTransform(0, 0);
    }
}
