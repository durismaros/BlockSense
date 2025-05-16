using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BlockSense.Views;
using System.Globalization;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BlockSense.Models.User;
using BlockSense.Utilities;
using BlockSense.Services;
using System.Runtime.InteropServices;
using BlockSenseAPI.Models.TwoFactorAuth;

namespace BlockSense;

public partial class UserProfileView : UserControl
{
    private readonly UserInfoModel _userInfo;
    private readonly AdditionalUserInfoModel _additionalUserInfo;
    private readonly TwoFactorAuthService _twoFactorAuthService;
    private readonly ProfilePictureHandler _profilePictureHandler;
    private readonly MainView _mainView;

    private InviteManagerWindow? _inviteManagerWindow;

    public UserProfileView(UserInfoModel userInfo, AdditionalUserInfoModel additionalUserInfo, TwoFactorAuthService twoFactorAuthService, ProfilePictureHandler profilePictureHandler, MainView mainView)
    {
        _userInfo = userInfo;
        _additionalUserInfo = additionalUserInfo;
        _twoFactorAuthService = twoFactorAuthService;
        _profilePictureHandler = profilePictureHandler;
        _mainView = mainView;

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
        if (_userInfo.UserId != 0)
        {
            ProfilePictureImage.Source = _profilePictureHandler.GetExistingPicture();

            UidTextBlock.Text = _userInfo.UserId.ToString();
            UsernameTextBlock.Text = _userInfo.Username;
            EmailTextBlock.Text = _userInfo.Email;

            switch (_userInfo.Type)
            {
                case UserType.User:
                    AccountBadgesPanel.Children.Add(userBadge);
                    break;

                case UserType.Admin:
                    AccountBadgesPanel.Children.Add(userBadge);
                    AccountBadgesPanel.Children.Add(adminBadge);
                    break;
            }

            CreationDateTextBlock.Text = SystemUtils.DateTransform(_userInfo.CreatedAt);

            InvitationUserTextBlock.Text = _userInfo.InvitingUser;

            LastUpdateTextBlock.Text = SystemUtils.DateTransform(_userInfo.UpdatedAt);

            int invitedUsers = _additionalUserInfo.InvitedUsers;
            UsersInvitedTextBlock.Text = $"{invitedUsers.ToString()} {(invitedUsers > 1 ? "Users" : "User")}";

            int activeDevices = _additionalUserInfo.ActiveDevices;
            ActiveDevicesTextBlock.Text = $"{activeDevices.ToString()} {(activeDevices > 1 ? "Devices" : "Device")}";

            bool TwoFaStatus = _additionalUserInfo.TwoFaEnabled;
            TwoFactorAuthTextBlock.Text = TwoFaStatus ? "Enabled" : "Disabled";
        }

        _mainView = mainView;
    }

    private async void InviteManagerClick(object sender, RoutedEventArgs e)
    {
        if (_inviteManagerWindow == null || _inviteManagerWindow.IsVisible == false)
        {
            _inviteManagerWindow = App.Services.GetRequiredService<InviteManagerWindow>();
            _inviteManagerWindow.Show();
            // Fade in animation on Window open
            await AnimationManager.FadeInAnimation.RunAsync(_inviteManagerWindow);
        }
    }

    private async void OpenSecurityManagerClick(object sender, RoutedEventArgs e)
    {
        var (setupKey, qrCodeBitmap) = await _twoFactorAuthService.DisplayAuthSetup();

        QRCodeImage.Source = qrCodeBitmap;
        SetupKeyText.Text = setupKey;

        SecurityManager.IsVisible = true;
        await AnimationManager.FadeInAnimation.RunAsync(SecurityManager);
    }

    private async void ToggleTwoFaClick(object sender, RoutedEventArgs e)
    {
        if (VerificationCodeInput.Text.Length != 6)
            return;

        var verification = await _twoFactorAuthService.CompleteTwoFaSetup(new TwoFactorSetupRequestModel
        {
            Code = VerificationCodeInput.Text,
            SecretKey = SetupKeyText.Text,
        });

        Console.WriteLine(verification);
    }

    private async void VerifyTwoFaClick(object sender, RoutedEventArgs e)
    {
        if (VerificationCodeInput.Text.Length != 6)
            return;

        var verification = await _twoFactorAuthService.VerifyOtp(new TwoFactorVerificationRequest
        {
            Code = VerificationCodeInput.Text
        });

        Console.WriteLine(verification.Verification);
        Console.WriteLine(verification.Message);
    }

    private async void CloseSecurityManagerClick(object sender, RoutedEventArgs e)
    {
        await AnimationManager.FadeOutAnimation.RunAsync(SecurityManager);
        SecurityManager.IsVisible = false;
    }

    private async void HomeClick(object sender, RoutedEventArgs e)
    {
        if (_inviteManagerWindow?.IsVisible == true)
        {
            _inviteManagerWindow.Close();
            await AnimationManager.FadeOutAnimation.RunAsync(_inviteManagerWindow);
            _inviteManagerWindow = null; // Clear reference to avoid memory leaks
        }
        //await MainWindow.SwitchView(new WelcomeView());
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
        //await MainWindow.SwitchView(_mainView);
    }
}