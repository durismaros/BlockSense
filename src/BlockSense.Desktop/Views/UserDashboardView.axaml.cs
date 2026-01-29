using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using BlockSense.Contracts.Enums.User;
using BlockSense.Desktop.Providers.Implementations;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.X509;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace BlockSense.Desktop;

public partial class UserDashboardView : UserControl
{
    private readonly ITwoFactorAuthService _twoFactorAuthService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly NavigationManager _navigationManager;
    private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

    public UserDashboardView()
    {
        _twoFactorAuthService = App.ServiceProvider.GetRequiredService<ITwoFactorAuthService>()
            ?? throw new ArgumentNullException(nameof(ITwoFactorAuthService));

        _currentUserProvider = App.ServiceProvider.GetRequiredService<ICurrentUserProvider>()
            ?? throw new ArgumentNullException(nameof(ICurrentUserProvider));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _twoFactorSlidingPanel = MainWindow.Instance.TwoFactorSlidingPanel
            ?? throw new ArgumentNullException(nameof(MainWindow.Instance.TwoFactorSlidingPanel));

        InitializeComponent();

        _currentUserProvider.OnCurrentUserChanged += OnCurrentUserChanged;

        HomeButton.Click += ToHomeViewClick;

        ManageSecuritySettingsButton.Click += OpenSecurityManagerClick;
        CloseSecuriyManagerButton.Click += CloseSecurityManagerClick;

        ManageActiveDevicesButton.Click += OpenDeviceManagerClick;
        CloseDeviceManagerButton.Click += CloseDeviceManagerClick;

        EnableTwoFactorButton.PointerPressed += EnableTwoFactorClick;

        GenerateBackupCodesButton.Click += GenerateBackupCodesButtonClick;
        DownloadBackupCodesButton.Click += DownloadBackupCodesButtonClick;

        DisableTwoFactorCheckButton.Click += DisableTwoFactorCheckButtonClick;
        DisableTwoFactorButton.Click += DisableTwoFactorClick;

        _twoFactorSlidingPanel.TwoFactorCodeSubmitted += async code =>
        {
            await OnTwoFactorCodeSubmitted(code);
        };
    }

    private void OnCurrentUserChanged()
    {
        UsernameTextBlock.Text =
            _currentUserProvider.Profile.Username;

        EmailTextBlock.Text =
            _currentUserProvider.Profile.Email;

        UserIdTextBlock.Text =
            _currentUserProvider.Profile.UserId.ToString();

        SetAccountBadges(_currentUserProvider.Profile.UserType);

        CreationDateTextBlock.Text =
            ToOrdinalDate(_currentUserProvider.Profile.CreatedAt);

        InvitedByTextBlock.Text =
            _currentUserProvider.Profile.InvitedBy;

        UpdatedAtTextBlock.Text =
            $"Updated: {ToOrdinalDate(_currentUserProvider.Profile.UpdatedAt)}";

        TwoFaStatusTextBlock.Text =
            _currentUserProvider.Profile.TwoFactorEnabled ? "Enabled" : "Disabled";

        UpdateSecurityManagerCard(_currentUserProvider.Profile.TwoFactorEnabled);

        ActiveDevicesTextBlock.Text =
            FormatDeviceCount(_currentUserProvider.ActiveDevices.Count);

        TotalInvitedUsersTextBlock.Text =
            FormatInvitationCount(_currentUserProvider.Invitations.Count);
    }

    private async void ToHomeViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<HomeView>();
    }

    private async void OpenSecurityManagerClick(object? sender, RoutedEventArgs e)
    {
        SecurityManagerCard.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(SecurityManagerCard);
    }
        
    private async void CloseSecurityManagerClick(object? sender, RoutedEventArgs e)
    {
        await Animations.FadeOutAnimation.RunAsync(SecurityManagerCard);
        SecurityManagerCard.IsVisible = false;
    }

    private async void OpenDeviceManagerClick(object? sender, RoutedEventArgs e)
    {
        DeviceManagerCard.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(DeviceManagerCard);
    }

    private async void CloseDeviceManagerClick(object? sender, RoutedEventArgs e)
    {
        await Animations.FadeOutAnimation.RunAsync(DeviceManagerCard);
        DeviceManagerCard.IsVisible = false;
    }

    private void EnableTwoFactorClick(object? sender, RoutedEventArgs e)
    {
        _twoFactorSlidingPanel.ShowPanel(TwoFactorPurpose.Enable);
    }

    private async void GenerateBackupCodesButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DownloadBackupCodesButton.IsVisible)
            return;

        string defaultText = BackupCodesTextBlock.Text ?? string.Empty;

        var response = await _twoFactorAuthService.GenerateBackupCodesAsync();

        if (response.ProblemType is ApiProblemTypes.TwoFactorAuthentication.TwoFactorAuthenticationSuccess)
        {
            DownloadBackupCodesButton.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(DownloadBackupCodesButton);
        }

        await Animations.FadeOutAnimation.RunAsync(BackupCodesTextBlock);
        BackupCodesTextBlock.Text = response.Message;
        await Animations.FadeInAnimation.RunAsync(BackupCodesTextBlock);

        // After delay, restore default
        await Task.Delay(5000);

        await Animations.FadeOutAnimation.RunAsync(BackupCodesTextBlock);
        BackupCodesTextBlock.Text = defaultText;
        await Animations.FadeInAnimation.RunAsync(BackupCodesTextBlock);
    }

    private async void DownloadBackupCodesButtonClick(object? sender, RoutedEventArgs e)
    {
        var backupCodes = _currentUserProvider.TwoFactorBackupCodes;

        if (backupCodes is null || backupCodes.Count is 0)
            return;

        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storageProvider)
            return;

        var saveOptions = new FilePickerSaveOptions
        {
            Title = "Save Backup Codes",
            SuggestedFileName = "*.txt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Text File")
                {
                    Patterns = new[] { "*.txt" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        };
        var file = await storageProvider.SaveFilePickerAsync(saveOptions);

        if (file is null)
            return;

        var content = string.Join(
            Environment.NewLine,
            backupCodes);

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
    }

    private async void DisableTwoFactorCheckButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DisableTwoFactorButton.IsVisible)
            return;

        DisableTwoFactorButton.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(DisableTwoFactorButton);
    }

    private void DisableTwoFactorClick(object? sender, RoutedEventArgs e)
    {
        _twoFactorSlidingPanel.ShowPanel(TwoFactorPurpose.Disable);
    }

    private async Task OnTwoFactorCodeSubmitted(string code)
    {
        try
        {
            bool success = _twoFactorSlidingPanel.Purpose switch
            {
                TwoFactorPurpose.Enable => await _twoFactorAuthService.EnableAsync(new TwoFactorSetupRequest
                {
                    SecretKey = SetupKeyTextBlock.Text ?? string.Empty,
                    TwoFactorCode = code
                }),
                TwoFactorPurpose.Disable => await _twoFactorAuthService.DisableAsync(new TwoFactorVerificationRequest
                {
                    TwoFactorCode = code
                }),
                _ => false
            };

            if (!success)
            {
                await _twoFactorSlidingPanel.ShowErrorState();
                return;
            }

            await _twoFactorSlidingPanel.ShowVerifiedState();
        }
        catch
        {
            await _twoFactorSlidingPanel.ShowErrorState();
        }
    }

    private async void UpdateSecurityManagerCard(bool twoFactorEnabled)
    {
        if (twoFactorEnabled)
        {
            TwoFactorEnabledContent.IsVisible = true;
            TwoFactorDisabledContent.IsVisible = false;

            if (_currentUserProvider.TwoFactorBackupCodes is not null &&
                _currentUserProvider.TwoFactorBackupCodes.Count > 0)
            {
                DownloadBackupCodesButton.IsVisible = true;
            }

            return;
        }

        TwoFactorEnabledContent.IsVisible = false;
        TwoFactorDisabledContent.IsVisible = true;

        var setup = await _twoFactorAuthService.GetSetupInitAsync();

        using var ms = new MemoryStream(setup.QRCodeData);
        TwoFactorQRCodeImage.Source = new Bitmap(ms);

        SetupKeyTextBlock.Text = setup.SetupKey;
    }

    private void SetAccountBadges(UserType userType)
    {
        AccountBadgesStackPanel.Children.Clear();

        switch (userType)
        {
            case UserType.Standard:
                AccountBadgesStackPanel.Children.Add(CreateBadge("user"));
                break;

            case UserType.Administrator:
                AccountBadgesStackPanel.Children.Add(CreateBadge("user"));
                AccountBadgesStackPanel.Children.Add(CreateBadge("admin"));
                break;

            case UserType.Founder:
                AccountBadgesStackPanel.Children.Add(CreateBadge("user"));
                AccountBadgesStackPanel.Children.Add(CreateBadge("admin"));
                AccountBadgesStackPanel.Children.Add(CreateBadge("founder"));
                break;
        }

        Border CreateBadge(string text)
        {
            return new Border()
            {
                Classes = { "badge" },
                Child = new TextBlock()
                {
                    Classes = { "badgeText" },
                    Text = text
                }
            };
        }
    }

    private static string ToOrdinalDate(DateTime date)
        => $"{date.ToString("MMM", CultureInfo.InvariantCulture)} {date.Day.Ordinalize()}, {date:yyyy}";

    private static string FormatDeviceCount(int count)
        => $"{count} {(count == 1 ? "Device" : "Devices")}";

    private static string FormatInvitationCount(int count)
    => $"{count} {(count != 1 ? "Users" : "User")}";
}