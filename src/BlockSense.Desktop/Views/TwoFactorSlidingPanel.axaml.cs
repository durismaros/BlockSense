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
using System.Runtime.InteropServices;
using System.Text;
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

    public event Func<string, Task>? TwoFactorCodeSubmitted;

    public bool IsPanelVisible
    {
        get;
        private set;
    }

    public TwoFactorSlidingPanel()
    {
        InitializeComponent();

        SetupCodeDigits();

        IsPanelVisible = false;

        // Initialize sliding panel position
        this.RenderTransform = new TranslateTransform(0, -450);

        DragBorder.PointerPressed += DragWindow;
        CancelCodeButton.Click += HidePanel;
        BackUpToggleButton.Click += ToggleBackupCodeModeClick;

        this.Focusable = true;
        this.KeyDown += OnKeyDown;
        this.Loaded += (s, e) => this.Focus();
    }

    public async void ShowPanel(object? sender, RoutedEventArgs? e)
    {
        await ShowDefaultState();
        await ResetCodeEntry();
        await AnimatePanel(true);

        IsPanelVisible = true;
    }

    public async void HidePanel(object? sender, RoutedEventArgs? e)
    {
        await AnimatePanel(false);

        IsPanelVisible = false;
    }

    public async Task ShowVerifiedState()
    {
        if (VerifiedStatePanel.IsVisible)
        {
            return;
        }

        VerifiedStatePanel.IsVisible = true;
        await Animations.FadeOutAnimation.RunAsync(InstructionsPanel);
        await Animations.FadeInAnimation.RunAsync(VerifiedStatePanel);
        InstructionsPanel.IsVisible = false;

        await Task.Delay(2000);
        await ResetCodeEntry();
        HidePanel(default, default);
    }

    public async Task ShowErrorState()
    {
        if (ErrorStatePanel.IsVisible)
        {
            return;
        }

        ErrorStatePanel.IsVisible = true;
        await Animations.FadeOutAnimation.RunAsync(InstructionsPanel);
        await Animations.FadeInAnimation.RunAsync(ErrorStatePanel);
        InstructionsPanel.IsVisible = false;

        await Task.Delay(1000);
        await ResetCodeEntry();
        await ShowDefaultState();
    }

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

    private async void VerifyCodeClick(object? sender, RoutedEventArgs e)
    {
        int codeLength = _isBackupMode ? BACKUP_CODE_LENGTH : CODE_LENGTH;

        if (_currentCode.Length != codeLength)
        {
            return;
        }

        // Format backup code with dash for verification
        string codeToVerify = _isBackupMode ?
            $"{_currentCode.Substring(0, 4)}-{_currentCode.Substring(4, 3)}" :
            _currentCode;

        if (TwoFactorCodeSubmitted is not null)
        {
            await TwoFactorCodeSubmitted.Invoke(codeToVerify);
        }
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsPanelVisible)
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
            HidePanel(default, default);
        }
    }

    private async Task ResetCodeEntry()
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
    }

    private async Task ShowDefaultState()
    {
        if (!ErrorStatePanel.IsVisible && !VerifiedStatePanel.IsVisible)
        {
            return;
        }

        await Animations.FadeOutAnimation.RunAsync(ErrorStatePanel);
        await Animations.FadeOutAnimation.RunAsync(VerifiedStatePanel);

        InstructionsPanel.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(InstructionsPanel);

        ErrorStatePanel.IsVisible = false;
        VerifiedStatePanel.IsVisible = false;
    }

    private async Task AnimatePanel(bool show)
    {
        if ((show && IsPanelVisible) || (!show && !IsPanelVisible))
            return;

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
                    Setters = { new Setter(TranslateTransform.YProperty, show ? 0.0 : -this.Bounds.Height) }
                }
            }
        };

        await animation.RunAsync(this);
    }

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

    private char? GetInputChar(Key key)
    {
        if (key >= Key.D0 && key <= Key.D9)
            return (char)('0' + (key - Key.D0));

        if (_isBackupMode && key >= Key.A && key <= Key.Z)
            return (char)('A' + (key - Key.A));

        return null;
    }

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

        // First 4 digits
        for (int i = 0; i < 4; i++)
        {
            _backupCodeDigits[i] = new Border
            {
                Classes = { "CodeDigit" },
                Child = new TextBlock
                {
                    Classes = { "DigitText" },
                    Text = ""
                }
            };
            firstGroup?.Children.Add(_backupCodeDigits[i]);
        }

        // Last 3 digits
        for (int i = 4; i < BACKUP_CODE_LENGTH; i++)
        {
            _backupCodeDigits[i] = new Border
            {
                Classes = { "CodeDigit" },
                Child = new TextBlock
                {
                    Classes = { "DigitText" },
                    Text = ""
                }
            };
            secondGroup?.Children.Add(_backupCodeDigits[i]);
        }
    }

    private void DragWindow(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && VisualRoot is Window window)
            window.BeginMoveDrag(e);
    }
}