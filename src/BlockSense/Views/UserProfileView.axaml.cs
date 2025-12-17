using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using BlockSense.Models.TwoFactorAuth.BackupCode;
using BlockSense.Models.TwoFactorAuth.Setup;
using BlockSense.Models.TwoFactorAuth.Verification;
using BlockSense.Models.User;
using BlockSense.Services;
using BlockSense.Utilities;
using BlockSense.Utilities.UI;
using BlockSense.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlockSense;

public partial class UserProfileView : UserControl
{
    private readonly UserInfo _userInfo;
    private readonly AdditionalUserInfo _additionalUserInfo;
    private readonly TwoFactorBackupCodes _twoFactorBackupCodes;

    private readonly TwoFactorAuthService _twoFactorAuthService;
    private readonly ProfilePictureHandler _profilePictureHandler;

    private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;
    private InviteManagerWindow? _inviteManagerWindow;

    private readonly AsyncDebouncer _debouncer;

    public UserProfileView(UserInfo userInfo, AdditionalUserInfo additionalUserInfo, TwoFactorBackupCodes twoFactorBackupCodes, TwoFactorAuthService twoFactorAuthService, ProfilePictureHandler profilePictureHandler, TwoFactorSlidingPanel twoFactorSlidingPanel)
    {
        _userInfo = userInfo;
        _additionalUserInfo = additionalUserInfo;
        _twoFactorBackupCodes = twoFactorBackupCodes;
        _twoFactorAuthService = twoFactorAuthService;
        _profilePictureHandler = profilePictureHandler;
        _twoFactorSlidingPanel = twoFactorSlidingPanel;

        _debouncer = new AsyncDebouncer();

        var userBadge = new Border()
        {
            Classes = { "badge" },
            Child = new TextBlock()
            {
                Classes = { "badgeText" },
                Text = "user"
            }
        };

        var adminBadge = new Border()
        {
            Classes = { "badge" },
            Child = new TextBlock()
            {
                Classes = { "badgeText" },
                Text = "admin"
            }
        };

        InitializeComponent();

        MainPanel.Children.Add(_twoFactorSlidingPanel);
        _twoFactorSlidingPanel.CodeSubmitted += OnTwoFactorCodeSubmitted;

        if (_userInfo.UserId != -1)
        {
            ProfilePictureImage.Source = _profilePictureHandler.GetExistingPicture();

            UidTextBlock.Text = _userInfo.UserId.ToString();
            UsernameTextBlock.Text = _userInfo.Username;
            EmailTextBlock.Text = _userInfo.Email;

            switch (_userInfo.Type)
            {
                case UserType.Standard:
                    AccountBadgesPanel.Children.Add(userBadge);
                    break;

                case UserType.Administrator:
                    AccountBadgesPanel.Children.Add(userBadge);
                    AccountBadgesPanel.Children.Add(adminBadge);
                    break;
            }

            CreationDateTextBlock.Text = SystemUtils.DateTransform(_userInfo.CreatedAt);

            InvitationUserTextBlock.Text = _userInfo.InvitingUser;

            LastUpdateTextBlock.Text = $"Updated: {SystemUtils.DateTransform(_userInfo.UpdatedAt)}";

            int invitedUsers = _additionalUserInfo.InvitedUsers;
            UsersInvitedTextBlock.Text = $"{invitedUsers.ToString()} {(invitedUsers != 1 ? "Users" : "User")}";

            int activeDevices = _additionalUserInfo.ActiveDevices;
            ActiveDevicesTextBlock.Text = $"{activeDevices.ToString()} {(activeDevices > 1 ? "Devices" : "Device")}";

            // Only generate QR code and setup key if 2FA is disabled and not already generated
            bool TwoFaStatus = _additionalUserInfo.TwoFaEnabled;
            UpdateTwoFaContent();

            if (_twoFactorBackupCodes.Codes.Count > 0)
                BackupCodesDownloadButton.IsVisible = true;
        }
    }

    public async void DownloadBackupCodesClick(object sender, RoutedEventArgs e)
    {
        if (_twoFactorBackupCodes?.Codes is null || _twoFactorBackupCodes.Codes.Count == 0)
            return;

        // Get the StorageProvider from the visual tree
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
            return;

        // Show the "Save As" picker
        var fileDialog = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Backup Codes",
            SuggestedFileName = "*.txt",
            FileTypeChoices = new[]
            {
            new FilePickerFileType("Text File") { Patterns = new[] { "*.txt" } },
            new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (fileDialog is not null)
        {
            string fileContent = string.Join(Environment.NewLine, _twoFactorBackupCodes.Codes);
            await using var stream = await fileDialog.OpenWriteAsync();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(fileContent);
        }
    }

    public async Task GenerateBackupCodes()
    {
        try
        {
            var success = await _twoFactorAuthService.GenerateBackupCodes();

            if (success)
            {
                BackupCodesDownloadButton.IsVisible = true;
                await AnimationManager.FadeInAnimation.RunAsync(BackupCodesDownloadButton);
                return;
            }

            await ShowTemporaryMessage(BackupCodesTextBlock, "You can generate new backup codes every 2 hours, please wait before trying again.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating backup codes: {ex.Message}");
        }
    }

    private void DragWindow(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && VisualRoot is Window window)
            window.BeginMoveDrag(e);
    }

    private async void OnTwoFactorCodeSubmitted(object? sender, TwoFactorCodeEventArgs e)
    {
        var executed = await _debouncer.TryExecuteAsync("verifyTwoFa", async () =>
        {
            await HandleCodeVerification(e);
        });
    }

    private async Task HandleCodeVerification(TwoFactorCodeEventArgs e)
    {
        try
        {
            bool verification = false;

            switch (e.Mode)
            {
                case TwoFactorSlidingPanel.TwoFactorMode.Enable:
                    verification = await _twoFactorAuthService.CompleteTwoFaSetup(new TwoFactorSetupRequest
                    {
                        Code = e.Code,
                        SecretKey = SetupKeyText.Text
                    });
                    break;

                case TwoFactorSlidingPanel.TwoFactorMode.Disable:
                    verification = await _twoFactorAuthService.DisableTwoFa(new TwoFactorVerificationRequest
                    {
                        Code = e.Code
                    });
                    break;
            }

            if (!verification)
            {
                await _twoFactorSlidingPanel.ShowError();
                return;
            }

            if (e.Mode == TwoFactorSlidingPanel.TwoFactorMode.Enable || e.Mode == TwoFactorSlidingPanel.TwoFactorMode.Disable)
            {
                _additionalUserInfo.TwoFaEnabled = (e.Mode == TwoFactorSlidingPanel.TwoFactorMode.Enable);
                UpdateTwoFaContent();
            }

            // Show success state in panel
            await _twoFactorSlidingPanel.ShowSuccessState();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying 2FA code: {ex.Message}");
            await _twoFactorSlidingPanel.ShowError();
        }
    }

    private async Task ShowTemporaryMessage(TextBlock textBlock, string message)
    {
        string defaultText = textBlock.Text ?? string.Empty;

        await AnimationManager.FadeOutAnimation.RunAsync(textBlock);
        textBlock.Text = message;
        await AnimationManager.FadeInAnimation.RunAsync(textBlock);

        // After delay, restore default
        await Task.Delay(5000);
        await AnimationManager.FadeOutAnimation.RunAsync(textBlock);
        textBlock.Text = defaultText;
        await AnimationManager.FadeInAnimation.RunAsync(textBlock);
    }

    private async void InviteManagerClick(object sender, RoutedEventArgs e)
    {
        if (_inviteManagerWindow == null || _inviteManagerWindow.IsVisible == false)
        {
            _inviteManagerWindow = App.Services!.GetRequiredService<InviteManagerWindow>();
            _inviteManagerWindow.Show();

            // Fade in animation on Window open
            await AnimationManager.FadeInAnimation.RunAsync(_inviteManagerWindow);
        }
    }

    private async void OpenSecurityManagerClick(object sender, RoutedEventArgs e)
    {
        SecurityManager.IsVisible = true;
        await AnimationManager.FadeInAnimation.RunAsync(SecurityManager);
    }

    private async void CloseSecurityManagerClick(object sender, RoutedEventArgs e)
    {
        await AnimationManager.FadeOutAnimation.RunAsync(SecurityManager);
        SecurityManager.IsVisible = false;
    }

    private async void EnableTwoFaClick(object sender, PointerPressedEventArgs e)
    {
        await _twoFactorSlidingPanel.ShowPanel(TwoFactorSlidingPanel.TwoFactorMode.Enable);
    }

    private async void GenerateBackupCodesClick(object sender, RoutedEventArgs e)
    {
        await _debouncer.TryExecuteAsync("generateBackup", GenerateBackupCodes);
    }

    private async void DisableTwoFaClick(object sender, RoutedEventArgs e)
    {
        if (DisableTwoFaButton.IsVisible)
            return;

        DisableTwoFaButton.IsVisible = true;
        await AnimationManager.FadeInAnimation.RunAsync(DisableTwoFaButton);
    }

    private async void ConfirmDisableTwoFaClick(object sender, RoutedEventArgs e)
    {
        await _twoFactorSlidingPanel.ShowPanel(TwoFactorSlidingPanel.TwoFactorMode.Disable);

        await AnimationManager.FadeOutAnimation.RunAsync(DisableTwoFaButton);
        DisableTwoFaButton.IsVisible = false;
    }

    private async void OpenDeviceManagerClick(object sender, RoutedEventArgs e)
    {
        DeviceManager.IsVisible = true;
        await AnimationManager.FadeInAnimation.RunAsync(DeviceManager);
    }
    private async void CloseDeviceManagerClick(object sender, RoutedEventArgs e)
    {
        await AnimationManager.FadeOutAnimation.RunAsync(DeviceManager);
        DeviceManager.IsVisible = false;
    }

    private void RevokeAccessClick(object sender, RoutedEventArgs e)
    {
        // Implement device revocation logic
    }

    private void LogOutAllDevicesClick(object sender, RoutedEventArgs e)
    {
        // Implement logout all devices logic
    }

    private async void HomeClick(object sender, RoutedEventArgs e)
    {
        if (_inviteManagerWindow?.IsVisible == true)
        {
            _inviteManagerWindow.Close();
            await AnimationManager.FadeOutAnimation.RunAsync(_inviteManagerWindow);
            _inviteManagerWindow = null; // Clear reference to avoid memory leaks
        }

        //await _viewSwitcher.NavigateToAsync<WelcomeView>();
    }

    private async void PfpUploadClick(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(sender as Avalonia.Visual).Properties.IsLeftButtonPressed)
        {
            var parentWindow = this.VisualRoot as Window;
            await _profilePictureHandler.UploadFile(parentWindow!);
            ProfilePictureImage.Source = _profilePictureHandler.GetExistingPicture();
        }
    }

    private void SetDefaultClick(object sender, RoutedEventArgs e)
    {
        ProfilePictureImage.Source = _profilePictureHandler.SetDefaultPfp();
    }

    private async void LogoutClick(object sender, RoutedEventArgs e)
    {
        //await UserService.Logout();
        //await _viewSwitcher.NavigateToAsync<MainView>();
    }

    private async void GenerateNewTwoFaSetup()
    {
        try
        {
            var authSetup = await _twoFactorAuthService.DisplayAuthSetup();

            SetupKeyText.Text = authSetup.setupKey;
            QRCodeImage.Source = authSetup.qrCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating 2FA setup: {ex.Message}");
        }
    }

    private void UpdateTwoFaContent()
    {
        if (TwoFaDisabledContent is not null && TwoFaEnabledContent is not null)
        {
            TwoFaDisabledContent.IsVisible = !_additionalUserInfo.TwoFaEnabled;
            TwoFaEnabledContent.IsVisible = _additionalUserInfo.TwoFaEnabled;
        }

        TwoFactorAuthTextBlock.Text = (_additionalUserInfo.TwoFaEnabled) ? "Enabled" : "Disabled";

        if (!_additionalUserInfo.TwoFaEnabled)
            GenerateNewTwoFaSetup();
    }
}