using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Desktop.Utilities.UIComponents;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class TwoFactorSlidingPanel : UserControl
{
    private const int CODE_LENGTH = 6;
    private const int BACKUP_CODE_LENGTH = 7;

    // UI Elements
    private readonly Border[] _regularCodeDigits = new Border[CODE_LENGTH];
    private readonly Border[] _backupCodeDigits = new Border[BACKUP_CODE_LENGTH];

    // 2FA Code Entry properties
    private string _currentCode = string.Empty;
    private bool _isBackupMode = false;

    // Code Submission Event
    public Func<string, Task>? OnSubmitAsync
    {
        get;
        private set;
    }

    public TwoFactorSlidingPanel()
    {
        InitializeComponent();
        SetupCodeDigits();

        this.Focusable = true;

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    /// <summary>
    /// Displays the sliding panel, resets code entry, and shows the default instructions state.
    /// </summary>
    public async void ShowPanel(Func<string, Task> onSubmitAsync)
    {
        if (this.IsVisible)
        {
            return;
        }

        // Initialize sliding panel position
        this.RenderTransform = new TranslateTransform(0, -MainWindow.Instance.Height);
        BackUpToggleButton.IsVisible = true;

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

        await ShowDefaultState();
        await ResetCodeEntry();

        await animation;
    }

    /// <summary>
    /// Hides the sliding panel with animation.
    /// </summary>
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

    /// <summary>
    /// Shows the verified state UI after successful code entry, waits briefly, then resets and hides the panel.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ShowVerifiedState()
    {
        if (VerifiedStatePanel.IsVisible)
        {
            return;
        }

        VerifiedStatePanel.IsVisible = true;

        await Task.WhenAll(
            Animations.FadeOutAnimation.RunAsync(TitleText),
            Animations.FadeOutAnimation.RunAsync(SubtitleText)
        );

        await Animations.FadeInAnimation.RunAsync(VerifiedStatePanel);
        await Task.Delay(2000);

        HidePanel();

        await Task.WhenAll(
            ShowDefaultState(),
            ResetCodeEntry()
            );
    }

    /// <summary>
    /// Shows the error state UI when code verification fails, waits briefly, then resets to default state.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ShowErrorState()
    {
        if (ErrorStatePanel.IsVisible)
        {
            return;
        }

        ErrorStatePanel.IsVisible = true;

        await Task.WhenAll(
            Animations.FadeOutAnimation.RunAsync(TitleText),
            Animations.FadeOutAnimation.RunAsync(SubtitleText)
        );

        await Animations.FadeInAnimation.RunAsync(ErrorStatePanel);
        await Task.Delay(1000);

        await Task.WhenAll(
            ShowDefaultState(),
            ResetCodeEntry()
            );
    }

    /// <summary>
    /// Shows the default instructions state and hides error or verified states.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ShowDefaultState()
    {
        if (ErrorStatePanel.IsVisible)
        {
            await Animations.FadeOutAnimation.RunAsync(ErrorStatePanel);
            ErrorStatePanel.IsVisible = false;
        }

        if (VerifiedStatePanel.IsVisible)
        {
            await Animations.FadeOutAnimation.RunAsync(VerifiedStatePanel);
            VerifiedStatePanel.IsVisible = false;
        }

        await Task.WhenAll(
            Animations.FadeInAnimation.RunAsync(TitleText),
            Animations.FadeInAnimation.RunAsync(SubtitleText)
            );
    }

    /// <summary>
    /// Handles the Verify button click event.
    /// Submits the entered 2FA or backup code via the TwoFactorCodeSubmitted event.
    /// </summary>
    private async void VerifyCodeClick(object? sender, RoutedEventArgs e)
    {
        int codeLength = _isBackupMode ? BACKUP_CODE_LENGTH : CODE_LENGTH;

        if (_currentCode.Length != codeLength)
        {
            return;
        }

        if (OnSubmitAsync is null)
        {
            return;
        }

        string codeToVerify = _isBackupMode ?
            $"{_currentCode.Substring(0, 4)}-{_currentCode.Substring(4, 3)}" :
            _currentCode;

        await OnSubmitAsync(codeToVerify);
    }

    /// <summary>
    /// Toggles between regular authenticator code and backup code input mode.
    /// </summary>
    private async void ToggleBackupCodeModeClick(object? sender, RoutedEventArgs e)
    {
        var currentPanel = _isBackupMode ? BackupCodePanel : RegularCodePanel;

        // Fade out current UI elements
        await Task.WhenAll(
            Animations.FadeOutAnimation.RunAsync(InstructionsPanel),
            Animations.FadeOutAnimation.RunAsync(currentPanel),
            Animations.FadeOutAnimation.RunAsync(BackUpToggleButton)
        );

        currentPanel.IsVisible = false;

        // Toggle mode
        _isBackupMode = !_isBackupMode;

        // Select new panel based on mode
        await ResetCodeEntry();
        var newPanel = _isBackupMode ? BackupCodePanel : RegularCodePanel;
        newPanel.IsVisible = true;

        // Update texts
        BackupToggleText.Text = _isBackupMode ? "use authenticator code" : "verify using backup code";
        TitleText.Text = _isBackupMode ? "Enter Backup Code" : "Enter Verification Code";
        SubtitleText.Text = _isBackupMode
            ? "Please enter one of your saved backup codes"
            : "Please enter the 6-digit code from your authenticator app";

        // Fade in updated UI elements
        await Task.WhenAll(
            Animations.FadeInAnimation.RunAsync(InstructionsPanel),
            Animations.FadeInAnimation.RunAsync(newPanel),
            Animations.FadeInAnimation.RunAsync(BackUpToggleButton)
        );
    }

    /// <summary>
    /// Handles key press input for code entry, including digits, backspace, and escape.
    /// </summary>
    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!this.IsVisible)
        {
            return;
        }

        int codeLength = _isBackupMode ? BACKUP_CODE_LENGTH : CODE_LENGTH;

        char? inputChar = GetInputChar(e.Key);

        // Handle alphanumeric input
        if (inputChar.HasValue && _currentCode.Length < codeLength)
        {
            _currentCode += inputChar.Value;
            await AnimateDigitEntry(_currentCode.Length - 1);

            if (_currentCode.Length == codeLength)
            {
                VerifyCodeButton.IsEnabled = true;
            }
        }
        // Handle Backspace
        else if (e.Key == Key.Back && _currentCode.Length > 0)
        {
            int lastIndex = _currentCode.Length - 1;
            _currentCode = _currentCode.Substring(0, lastIndex);

            await AnimateDigitClear(lastIndex);

            VerifyCodeButton.IsEnabled = false;
        }
        // Handle Escape to cancel
        else if (e.Key == Key.Escape)
        {
            HidePanel();
        }
    }

    /// <summary>
    /// Resets the entered code and clears all displayed digit UI elements.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task ResetCodeEntry()
    {
        _currentCode = string.Empty;

        ClearDigits(_regularCodeDigits);
        ClearDigits(_backupCodeDigits);

        VerifyCodeButton.IsEnabled = false;

        void ClearDigits(Border[] digits)
        {
            foreach (var border in digits)
            {
                if (border.Child is TextBlock textBlock)
                {
                    textBlock.Text = string.Empty;
                }
            }
        }

        this.Focus();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Animates a single digit being entered: displays the character and plays a scale-up/scale-down effect.
    /// </summary>
    /// <param name="index">The zero-based index of the digit being entered.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task AnimateDigitEntry(int index)
    {
        Border[] digits = _isBackupMode ? _backupCodeDigits : _regularCodeDigits;

        if (index < 0 || index >= digits.Length)
        {
            return;
        }

        if (digits[index].Child is not TextBlock textBlock)
        {
            return;
        }

        textBlock.Text = _currentCode[index].ToString();

        var border = digits[index];
        var transform = new ScaleTransform();
        border.RenderTransform = transform;
        border.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

        // Scale up animation
        var scaleUpAnimation = new Animation
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
                        new Setter(ScaleTransform.ScaleXProperty, 1.1),
                        new Setter(ScaleTransform.ScaleYProperty, 1.1)
                    }
                }
            }
        };

        await scaleUpAnimation.RunAsync(border);

        // Scale down animation
        var scaleDownAnimation = new Animation
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

        await scaleDownAnimation.RunAsync(border);
    }

    /// <summary>
    /// Animates clearing a single digit: clears text and applies a brief fade animation.
    /// </summary>
    /// <param name="index">The zero-based index of the digit being cleared.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task AnimateDigitClear(int index)
    {
        Border[] digits = _isBackupMode ? _backupCodeDigits : _regularCodeDigits;

        if (index < 0 || index >= digits.Length)
        {
            return;
        }

        if (digits[index].Child is not TextBlock textBlock)
        {
            return;
        }

        textBlock.Text = string.Empty;

        var border = digits[index];

        textBlock.Text = string.Empty;

        // Fade animation
        var fadeAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(100),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Setter(Border.OpacityProperty, 0.5) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Setter(Border.OpacityProperty, 1.0) }
                }
            }
        };

        await fadeAnimation.RunAsync(border);
    }

    /// <summary>
    /// Maps a key press to a character for code entry (digits 0–9 and letters A–Z for backup codes).
    /// </summary>
    /// <param name="key">The key pressed by the user.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private char? GetInputChar(Key key)
    {
        if (key >= Key.D0 && key <= Key.D9)
        {
            return (char)('0' + (key - Key.D0));
        }

        if (_isBackupMode && key >= Key.A && key <= Key.Z)
        {
            return (char)('A' + (key - Key.A));
        }

        return null;
    }

    /// <summary>
    /// Sets up the visual UI elements (borders and textblocks) for both regular and backup code entry panels.
    /// </summary>
    private void SetupCodeDigits()
    {
        // Setup regular code digits (6-digit)
        for (int i = 0; i < CODE_LENGTH; i++)
        {
            _regularCodeDigits[i] = new Border
            {
                Classes = { "CodeDigit" },
                Child = new TextBlock
                {
                    Classes = { "DigitText" },
                    Text = ""
                }
            };
            RegularCodePanel.Children.Add(_regularCodeDigits[i]);
        }

        // Setup backup code digits (XXXX-XXX format)
        var firstGroup = BackupCodePanel.Children[0] as StackPanel;
        var secondGroup = BackupCodePanel.Children[2] as StackPanel;

        for (int i = 0; i < BACKUP_CODE_LENGTH; i++)
        {
            var digitBorder = new Border
            {
                Classes = { "CodeDigit" },
                Child = new TextBlock
                {
                    Classes = { "DigitText" },
                    Text = ""
                }
            };

            _backupCodeDigits[i] = digitBorder;

            // First 4 digits go to first group, last 3 to second group
            if (i < 4)
            {
                firstGroup?.Children.Add(digitBorder);
            }

            else
            {
                secondGroup?.Children.Add(digitBorder);
            }
        }
    }

    /// <summary>
    /// Allows dragging the parent window by holding the left mouse button on the panel's top area.
    /// </summary>
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
        CancelCodeButton.Click += HidePanel;
        BackUpToggleButton.Click += ToggleBackupCodeModeClick;
        VerifyCodeButton.Click += VerifyCodeClick;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        this.KeyDown -= OnKeyDown;
        DragBorder.PointerPressed -= DragWindow;
        CancelCodeButton.Click -= HidePanel;
        BackUpToggleButton.Click -= ToggleBackupCodeModeClick;
        VerifyCodeButton.Click -= VerifyCodeClick;
    }
}