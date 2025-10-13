using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using BlockSense.Models.TwoFactorAuth.Verification;
using BlockSense.Services;
using BlockSense.Utilities.UI;
using System;
using System.Threading.Tasks;

namespace BlockSense;

public partial class TwoFactorSlidingPanel : UserControl
{
    // Events for communication with parent
    public event EventHandler<TwoFactorCodeEventArgs>? CodeSubmitted;

    public TwoFactorMode CurrentMode { get; private set; } = TwoFactorMode.Verify;
    public bool IsPanelVisible { get; private set; } = false;

    private readonly AsyncDebouncer _debouncer;

    // 2FA Code Entry properties
    private const int CODE_LENGTH = 6;
    private const int BACKUP_CODE_LENGTH = 7; // Format: XXXX-XXX
    private string _currentCode = string.Empty;
    private bool _isBackupMode = false;

    // UI Elements
    private readonly Border[] _regularCodeDigits = new Border[CODE_LENGTH];
    private readonly Border[] _backupCodeDigits = new Border[BACKUP_CODE_LENGTH];

    public TwoFactorSlidingPanel(TwoFactorAuthService twoFactorAuthService)
    {
        _debouncer = new AsyncDebouncer();

        InitializeComponent();
        SetupCodeDigits();

        // Initialize sliding panel position
        SlidePanel.RenderTransform = new TranslateTransform(0, -450);

        // Setup focus and keyboard handling
        this.Focusable = true;
        this.KeyDown += OnKeyDown;
        this.Loaded += (s, e) => this.Focus();
    }

    public async Task ShowPanel(TwoFactorMode mode)
    {
        switch (mode)
        {
            case TwoFactorMode.Enable:
                TitleText.Text = "Enable Two-Factor Authentication";
                SubtitleText.Text = "Enter the code from your authenticator to enable 2FA";
                BackupCodeToggle.IsVisible = false;
                break;

            case TwoFactorMode.Disable:
                TitleText.Text = "Disable Two-Factor Authentication";
                SubtitleText.Text = "Enter the code to confirm disabling 2FA";
                BackupCodeToggle.IsVisible = true;
                break;

            default:
                TitleText.Text = "Enter Verification Code";
                SubtitleText.Text = "Please enter the 6-digit code from your authenticator app";
                BackupCodeToggle.IsVisible = true;
                break;
        }

        _isBackupMode = false;
        CurrentMode = mode;

        UpdateUIForMode();
        await ResetCodeEntry();
        await AnimatePanel(true);
        this.Focus();
    }

    public async Task HidePanel()
    {
        await AnimatePanel(false);
    }

    public async Task ShowSuccessState()
    {
        await ShowVerifiedState();
        await Task.Delay(2000);
        await HidePanel();
        await ClearPreviousState();
    }

    private void SetupCodeDigits()
    {
        // Setup regular code digits (6-digit)
        for (int i = 0; i < CODE_LENGTH; i++)
        {
            _regularCodeDigits[i] = new Border
            {
                Classes = { "codeDigit" },
                Child = new TextBlock
                {
                    Classes = { "digitText" },
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
                Classes = { "codeDigit" },
                Child = new TextBlock
                {
                    Classes = { "digitText" },
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
                Classes = { "codeDigit" },
                Child = new TextBlock
                {
                    Classes = { "digitText" },
                    Text = ""
                }
            };
            secondGroup?.Children.Add(_backupCodeDigits[i]);
        }
    }

    private void UpdateUIForMode()
    {
        RegularCodePanel.IsVisible = !_isBackupMode;
        BackupCodePanel.IsVisible = _isBackupMode;

        BackupCodeToggleText.Text = _isBackupMode ? "use authenticator code" : "verify using backup code";

        if (_isBackupMode && CurrentMode == TwoFactorMode.Verify)
        {
            TitleText.Text = "Enter Backup Code";
            SubtitleText.Text = "Please enter one of your backup codes (format: XXXX-XXX)";
        }
        else if (CurrentMode == TwoFactorMode.Disable)
        {
            TitleText.Text = "Disable Two-Factor Authentication";
            SubtitleText.Text = "Enter the code to confirm disabling 2FA";
        }
        else if (CurrentMode == TwoFactorMode.Verify)
        {
            TitleText.Text = "Enter Verification Code";
            SubtitleText.Text = "Please enter the 6-digit code from your authenticator app";
        }
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
                    Setters = { new Setter(TranslateTransform.YProperty, show ? 0.0 : -SlidePanel.Bounds.Height) }
                }
            }
        };

        await animation.RunAsync(SlidePanel);
        IsPanelVisible = show;
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsPanelVisible)
            return;

        int maxLength = _isBackupMode ? BACKUP_CODE_LENGTH : CODE_LENGTH;

        // Handle alphanumeric input
        if (IsValidInput(e.Key) && _currentCode.Length < maxLength)
        {
            char inputChar = GetCharFromKey(e.Key);

            // For backup codes, skip position 4 (dash position)
            if (_isBackupMode && _currentCode.Length == 4)
            {
                // Don't add the dash to _currentCode, just display it
            }

            _currentCode += inputChar;
            await AnimateDigitEntry(_currentCode.Length - 1);

            if (_currentCode.Length == maxLength)
            {
                VerifyCodeButton.IsEnabled = true;
                VerifyCodeButton.Opacity = 1.0;
            }
        }
        // Handle Backspace
        else if (e.Key == Key.Back && _currentCode.Length > 0)
        {
            int lastIndex = _currentCode.Length - 1;
            _currentCode = _currentCode.Substring(0, lastIndex);
            await AnimateDigitClear(lastIndex);

            VerifyCodeButton.IsEnabled = false;
            VerifyCodeButton.Opacity = 0.5;
        }
        // Handle Enter to verify
        else if (e.Key == Key.Enter && _currentCode.Length == maxLength)
        {
            SubmitCode();
        }
        // Handle Escape to cancel
        else if (e.Key == Key.Escape)
        {
            await HidePanel();
        }
    }

    private bool IsValidInput(Key key)
    {
        if (!_isBackupMode)
            return (key >= Key.D0 && key <= Key.D9);

        // Allow numbers and letters (A-Z)
        return (key >= Key.D0 && key <= Key.D9) || (key >= Key.A && key <= Key.Z);
    }

    private char GetCharFromKey(Key key)
    {
        if (key >= Key.D0 && key <= Key.D9)
            return (char)('0' + (key - Key.D0));

        if (key >= Key.A && key <= Key.Z)
            return (char)('A' + (key - Key.A));

        return '0'; // fallback
    }

    private async Task AnimateDigitEntry(int index)
    {
        Border[] digits = _isBackupMode ? _backupCodeDigits : _regularCodeDigits;

        if (index < 0 || index >= digits.Length)
            return;

        var border = digits[index];
        var textBlock = border.Child as TextBlock;

        if (textBlock is null)
            return;

        textBlock.Text = _currentCode[index].ToString();

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
            return;

        var border = digits[index];
        var textBlock = border.Child as TextBlock;

        if (textBlock is null)
            return;

        textBlock.Text = string.Empty;

        // Simple fade animation
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

    private async Task ResetCodeEntry()
    {
        _currentCode = string.Empty;

        // Clear regular digits
        for (int i = 0; i < _regularCodeDigits.Length; i++)
        {
            var textBlock = _regularCodeDigits[i].Child as TextBlock;
            if (textBlock is not null)
                textBlock.Text = string.Empty;
        }

        // Clear backup digits
        for (int i = 0; i < _backupCodeDigits.Length; i++)
        {
            var textBlock = _backupCodeDigits[i].Child as TextBlock;
            if (textBlock is not null)
                textBlock.Text = string.Empty;
        }

        VerifyCodeButton.IsEnabled = false;
        VerifyCodeButton.Opacity = 0.5;

        if (ErrorStatePanel.IsVisible)
            await ClearPreviousState();
    }

    private async Task ShowVerifiedState()
    {
        if (VerifiedStatePanel.IsVisible)
            return;

        VerifiedStatePanel.IsVisible = true;
        await AnimationManager.FadeOutAnimation.RunAsync(InstructionsPanel);
        await AnimationManager.FadeInAnimation.RunAsync(VerifiedStatePanel);
        InstructionsPanel.IsVisible = false;
    }

    private async Task ShowErrorState()
    {
        if (ErrorStatePanel.IsVisible)
            return;

        ErrorStatePanel.IsVisible = true;
        await AnimationManager.FadeOutAnimation.RunAsync(InstructionsPanel);
        await AnimationManager.FadeInAnimation.RunAsync(ErrorStatePanel);
        InstructionsPanel.IsVisible = false;
    }

    private async Task ClearPreviousState()
    {
        if (!ErrorStatePanel.IsVisible && !VerifiedStatePanel.IsVisible)
            return;

        InstructionsPanel.IsVisible = true;
        await AnimationManager.FadeOutAnimation.RunAsync(ErrorStatePanel);
        await AnimationManager.FadeOutAnimation.RunAsync(VerifiedStatePanel);
        await AnimationManager.FadeInAnimation.RunAsync(InstructionsPanel);

        ErrorStatePanel.IsVisible = false;
        VerifiedStatePanel.IsVisible = false;
    }

    private void SubmitCode()
    {
        int expectedLength = _isBackupMode ? BACKUP_CODE_LENGTH : CODE_LENGTH;

        if (_currentCode.Length != expectedLength)
            return;

        // Format backup code with dash for verification
        string codeToVerify = _isBackupMode ?
            $"{_currentCode.Substring(0, 4)}-{_currentCode.Substring(4, 3)}" :
            _currentCode;

        var eventArgs = new TwoFactorCodeEventArgs(codeToVerify, CurrentMode);
        CodeSubmitted?.Invoke(this, eventArgs);
    }

    public async Task ShowError()
    {
        await ShowErrorState();
        await Task.Delay(1000);
        await ResetCodeEntry();
    }

    private void DragWindow(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && VisualRoot is Window window)
            window.BeginMoveDrag(e);
    }

    private async void ToggleBackupCodeModeClick(object sender, RoutedEventArgs e)
    {
        var currentPanel = _isBackupMode ? BackupCodePanel : RegularCodePanel;

        if (CurrentMode == TwoFactorMode.Verify)
            await AnimationManager.FadeOutAnimation.RunAsync(InstructionsPanel);
        await AnimationManager.FadeOutAnimation.RunAsync(currentPanel);
        await AnimationManager.FadeOutAnimation.RunAsync(BackupCodeToggle);

        _isBackupMode = !_isBackupMode;
        await ResetCodeEntry();
        UpdateUIForMode();

        var nextPanel = _isBackupMode ? BackupCodePanel : RegularCodePanel;
        nextPanel.Opacity = 0;

        if (CurrentMode == TwoFactorMode.Verify)
            await AnimationManager.FadeInAnimation.RunAsync(InstructionsPanel);
        await AnimationManager.FadeInAnimation.RunAsync(nextPanel);
        await AnimationManager.FadeInAnimation.RunAsync(BackupCodeToggle);

        this.Focus();
    }


    private async void CancelClick(object sender, RoutedEventArgs e)
    {
        await HidePanel();
    }

    private void VerifyClick(object sender, RoutedEventArgs e)
    {
        SubmitCode();
    }

    public enum TwoFactorMode
    {
        Enable,
        Disable,
        Verify
    }
}

public class TwoFactorCodeEventArgs : EventArgs
{
    public string Code { get; }
    public TwoFactorSlidingPanel.TwoFactorMode Mode { get; }

    public TwoFactorCodeEventArgs(string code, TwoFactorSlidingPanel.TwoFactorMode mode)
    {
        Code = code;
        Mode = mode;
    }
}