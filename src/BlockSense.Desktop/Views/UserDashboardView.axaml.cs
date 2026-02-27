using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using BlockSense.Contracts.Enums;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.Formatting;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class UserDashboardView : UserControl
{
    private readonly ITwoFactorAuthService _twoFactorAuthService;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly NavigationManager _navigationManager;
    private readonly InvitationManagerWindow _invitationManagerWindow;

    public UserDashboardView()
    {
        _twoFactorAuthService = App.ServiceProvider.GetRequiredService<ITwoFactorAuthService>()
            ?? throw new ArgumentNullException(nameof(ITwoFactorAuthService));

        _tokenService = App.ServiceProvider.GetRequiredService<ITokenService>()
            ?? throw new ArgumentNullException(nameof(ITokenService));

        _currentUserProvider = App.ServiceProvider.GetRequiredService<ICurrentUserProvider>()
            ?? throw new ArgumentNullException(nameof(ICurrentUserProvider));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _invitationManagerWindow = App.ServiceProvider.GetRequiredService<InvitationManagerWindow>()
            ?? throw new ArgumentNullException(nameof(InvitationManagerWindow));

        InitializeComponent();

        _currentUserProvider.OnCurrentUserChanged += OnCurrentUserChanged;

        HomeButton.Click += ToHomeViewClick;

        OpenInvitationManagerButton.Click += OpenInvitationManagerButtonClick;

        ManageSecuritySettingsButton.Click += OpenSecurityManagerClick;
        CloseSecuriyManagerButton.Click += CloseSecurityManagerClick;

        ManageActiveDevicesButton.Click += OpenDeviceManagerClick;
        CloseDeviceManagerButton.Click += CloseDeviceManagerClick;

        EnableTwoFactorButton.PointerPressed += EnableTwoFactorClick;
        GenerateBackupCodesButton.Click += GenerateBackupCodesClick;
        DownloadBackupCodesButton.Click += DownloadBackupCodesClick;

        DisableTwoFactorCheckButton.Click += DisableTwoFactorCheckClick;
        DisableTwoFactorButton.Click += DisableTwoFactorClick;

        ViewFullActivityLogButton.Click += OpenActivityLogClick;

        ActivityLogOverlay.CloseRequested += CloseActivityLogAsync;
    }

    private void OnCurrentUserChanged()
    {
        UsernameTextBlock.Text =
            _currentUserProvider.Profile.Username;

        EmailTextBlock.Text =
            _currentUserProvider.Profile.Email;

        UserIdTextBlock.Text =
            _currentUserProvider.Profile.UserId.ToString();

        SetAccountBadges(_currentUserProvider.Profile.Role);

        CreationDateTextBlock.Text =
            DateTimeFormatter.ToOrdinalDate(_currentUserProvider.Profile.CreatedAt);

        InvitedByTextBlock.Text =
            _currentUserProvider.Profile.InvitedBy;

        UpdatedAtTextBlock.Text =
            $"Updated: {DateTimeFormatter.ToOrdinalDate(_currentUserProvider.Profile.UpdatedAt)}";

        TwoFaStatusTextBlock.Text =
            _currentUserProvider.Profile.TwoFactorEnabled ? "Enabled" : "Disabled";

        ActiveDevicesTextBlock.Text =
            FormatDeviceCount(_currentUserProvider.ActiveDevices.Count);

        TotalInvitedUsersTextBlock.Text =
            FormatInvitationCount(_currentUserProvider.Invitations.Count);

        UpdateSecurityManagerCard();

        UpdateActiveDevices();
    }

    private async void ToHomeViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<HomeView>();
    }

    private async void OpenInvitationManagerButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_invitationManagerWindow.IsVisible)
        {
            _invitationManagerWindow.Activate();
            return;
        }

        _invitationManagerWindow.Show();
        await Animations.FadeInAnimation.RunAsync(_invitationManagerWindow);
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

    private async void GenerateBackupCodesClick(object? sender, RoutedEventArgs e)
    {
        await _twoFactorAuthService.GenerateBackupCodesAsync();
    }

    private async void DownloadBackupCodesClick(object? sender, RoutedEventArgs e)
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

    private async void DisableTwoFactorCheckClick(object? sender, RoutedEventArgs e)
    {
        if (DisableTwoFactorButton.IsVisible)
        {
            return;
        }

        DisableTwoFactorButton.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(DisableTwoFactorButton);
    }

    private async void ConfirmRevokeDeviceClick(string tokenHash)
    {
        //_twoFactorSlidingPanel.ShowPanel(async code =>
        //{
        //    var request = new SessionRevokeRequest
        //    {
        //        TokenHash = tokenHash,
        //        TwoFactorCode = code
        //    };

        //    bool success = await _tokenService.RevokeAsync(request);
        //});
    }

    private async void EnableTwoFactorClick(object? sender, RoutedEventArgs e)
    {
        var setupKey = SetupKeyTextBlock.Text ?? string.Empty;

        await _twoFactorAuthService.EnableAsync(setupKey);
    }

    private async void DisableTwoFactorClick(object? sender, RoutedEventArgs e)
    {
        await _twoFactorAuthService.DisableAsync();
    }

    private void UpdateActiveDevices()
    {
        DevicesPanel.Children.Clear();

        foreach (var device in _currentUserProvider.ActiveDevices)
        {
            DevicesPanel.Children.Add(CreateDeviceCard(device));
        }
    }

    private async void UpdateSecurityManagerCard()
    {
        if (_currentUserProvider.Profile.TwoFactorEnabled)
        {
            ShowTwoFactorEnabledState();
            return;
        }

        await ShowTwoFactorDisabledStateAsync();
    }

    private void SetAccountBadges(UserRole role)
    {
        AccountBadgesStackPanel.Children.Clear();

        switch (role)
        {
            case UserRole.Standard:
                AccountBadgesStackPanel.Children.Add(CreateBadge("user"));
                break;

            case UserRole.Administrator:
                AccountBadgesStackPanel.Children.Add(CreateBadge("user"));
                AccountBadgesStackPanel.Children.Add(CreateBadge("admin"));
                break;

            case UserRole.Founder:
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

    private Control CreateDeviceCard(SessionDto device) 
    {
        // Left content: IP and dates
        var leftStack = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new ContentControl
                {
                    Classes = { "deviceIp" },
                    Tag = device.IpAddress
                }
            }
        };

        // Dates grid
        var datesGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("* *"),
        };

        var issuedAt = new ContentControl
        {
            Classes = { "deviceIssuedAt" },
            Tag = DateTimeFormatter.ToOrdinalDate(device.IssuedAt)
        };
        Grid.SetColumn(issuedAt, 0);

        var expiresAt = new ContentControl
        {
            Classes = { "deviceExpiresAt" },
            Tag = DateTimeFormatter.ToOrdinalDate(device.ExpiresAt)
        };
        Grid.SetColumn(expiresAt, 1);

        datesGrid.Children.Add(issuedAt);
        datesGrid.Children.Add(expiresAt);

        leftStack.Children.Add(datesGrid);

        // Main grid
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("* Auto")
        };

        grid.Children.Add(leftStack);

        // Revoke button
        var revokeButton = new Button
        {
            Classes = { "DefaultDisable" },
            Content = new TextBlock
            {
                Text = "Revoke",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse("#FF5733")),
                FontSize = 12,
                FontWeight = FontWeight.Medium
            }
        };
        Grid.SetColumn(revokeButton, 1);

        // Confirm button (initially hidden)
        var confirmButton = new Button
        {
            Classes = { "ConfirmDisable" },
            IsVisible = false,
            ZIndex = 1,
            Content = new TextBlock
            {
                Text = "Confirm",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse("#F5E1DA")),
                FontSize = 12,
                FontWeight = FontWeight.Medium
            }
        };
        Grid.SetColumn(confirmButton, 1);

        revokeButton.Click += async (s, e) =>
        {
            confirmButton.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(confirmButton);

            await Task.Delay(3000);

            await Animations.FadeOutAnimation.RunAsync(confirmButton);
            confirmButton.IsVisible = false;
        };


        confirmButton.Click += (s, e) =>
        {
            ConfirmRevokeDeviceClick(device.TokenHash);
        };

        grid.Children.Add(revokeButton);
        grid.Children.Add(confirmButton);

        // Root border
        return new Border
        {
            Classes = { "deviceCard" },
            Child = grid
        };
    }

    private void ShowTwoFactorEnabledState()
    {
        TwoFactorEnabledContent.IsVisible = true;
        TwoFactorDisabledContent.IsVisible = false;

        DownloadBackupCodesButton.IsVisible =
            _currentUserProvider.TwoFactorBackupCodes?.Count > 0;
    }

    private async Task ShowTwoFactorDisabledStateAsync()
    {
        TwoFactorEnabledContent.IsVisible = false;
        TwoFactorDisabledContent.IsVisible = true;

        var setup = await _twoFactorAuthService.GetSetupInitAsync();

        TwoFactorQRCodeImage.Source =
            new Bitmap(new MemoryStream(setup.QRCodeData));

        SetupKeyTextBlock.Text = setup.SetupKey;
    }

    private async void OpenActivityLogClick(object? sender, RoutedEventArgs e)
    {
        ActivityLogOverlay.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(ActivityLogOverlay);
    }

    private async Task CloseActivityLogAsync()
    {
        await Animations.FadeOutAnimation.RunAsync(ActivityLogOverlay);
        ActivityLogOverlay.IsVisible = false;
    }

    private static string FormatDeviceCount(int count)
        => $"{count} {(count == 1 ? "Device" : "Devices")}";

    private static string FormatInvitationCount(int count)
    => $"{count} {(count != 1 ? "Users" : "User")}";
}