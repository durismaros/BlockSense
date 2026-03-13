using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.Enums;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.Formatting;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class UserDashboardView : UserControl
{
    private readonly ITwoFactorAuthService _twoFactorAuthService;
    private readonly ITokenService _tokenService;
    private readonly ISessionService _sessionService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly NavigationManager _navigationManager;
    private readonly InvitationManagerWindow _invitationManagerWindow;

    public UserDashboardView()
    {
        _twoFactorAuthService = App.ServiceProvider.GetRequiredService<ITwoFactorAuthService>();
        _tokenService = App.ServiceProvider.GetRequiredService<ITokenService>();
        _sessionService = App.ServiceProvider.GetRequiredService<ISessionService>();
        _currentUserProvider = App.ServiceProvider.GetRequiredService<ICurrentUserProvider>();
        _refreshTokenProvider = App.ServiceProvider.GetRequiredService<IRefreshTokenProvider>();
        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>();
        _invitationManagerWindow = App.ServiceProvider.GetRequiredService<InvitationManagerWindow>();

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
        LogOutAllDevicesButton.Click += LogOutAllDevicesClick;
        ConfirmLogOutAllDevicesButton.Click += ConfirmLogOutAllDevicesClick;
        ViewFullActivityLogButton.Click += OpenActivityLogClick;
        ActivityLogOverlay.CloseRequested += CloseActivityLogAsync;
    }

    private void OnCurrentUserChanged()
    {
        UsernameTextBlock.Text = _currentUserProvider.Profile.Username;
        EmailTextBlock.Text = _currentUserProvider.Profile.Email;
        UserIdTextBlock.Text = _currentUserProvider.Profile.UserId.ToString();
        CreationDateTextBlock.Text = DateTimeFormatter.ToOrdinalDate(_currentUserProvider.Profile.CreatedAt);
        InvitedByTextBlock.Text = _currentUserProvider.Profile.InvitedBy;
        UpdatedAtTextBlock.Text = $"Updated: {DateTimeFormatter.ToOrdinalDate(_currentUserProvider.Profile.UpdatedAt)}";
        TwoFaStatusTextBlock.Text = _currentUserProvider.Profile.TwoFactorEnabled ? "Enabled" : "Disabled";

        ActiveDevicesTextBlock.Text =
            FormatDeviceCount(_currentUserProvider.ActiveDevices.Count);

        TotalInvitedUsersTextBlock.Text =
            FormatInvitationCount(_currentUserProvider.Invitations.Count(i => i.Status == InvitationStatus.Used));

        SetAccountBadges(_currentUserProvider.Profile.Role);
        UpdateRecentActivityCard();
        UpdateSecurityManagerCard();
        UpdateActiveDevices();
    }

    private void UpdateRecentActivityCard()
    {
        RecentActivityPanel.Children.Clear();

        var entries = _currentUserProvider.RecentActivity;

        if (entries.Count == 0)
        {
            RecentActivityPanel.Children.Add(new TextBlock
            {
                Text = "No recent activity.",
                Foreground = new SolidColorBrush(Color.Parse("#9E8572")),
                FontSize = 13,
                FontStyle = FontStyle.Italic,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 8)
            });
            return;
        }

        foreach (var log in entries)
        {
            RecentActivityPanel.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.Parse("#EDE7DE"))
            });

            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("160 * 90"),
                Margin = new Avalonia.Thickness(0, 8)
            };

            var date = new TextBlock
            {
                Text = DateTimeFormatter.ToOrdinalDate(log.OccurredAt),
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.Parse("#9E8572")),
                FontStyle = FontStyle.Italic,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(date, 0);

            var msg = new TextBlock
            {
                Text = log.ActivityMessage,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.Parse("#4A4238")),
                FontWeight = FontWeight.Medium,
                TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(12, 0)
            };
            Grid.SetColumn(msg, 1);

            var (bgHex, fgHex) = ActivityTypeColors(log.Type);
            var pill = new Border
            {
                CornerRadius = new Avalonia.CornerRadius(10),
                Padding = new Avalonia.Thickness(9, 3),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Color.Parse(bgHex)),
                Child = new TextBlock
                {
                    Text = log.Type.ToString().ToUpperInvariant(),
                    FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                    LetterSpacing = 0.5,
                    Foreground = new SolidColorBrush(Color.Parse(fgHex))
                }
            };
            Grid.SetColumn(pill, 2);

            row.Children.Add(date);
            row.Children.Add(msg);
            row.Children.Add(pill);
            RecentActivityPanel.Children.Add(row);
        }
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

    private async void ToHomeViewClick(object? sender, RoutedEventArgs e)
        => await _navigationManager.NavigateToAsync<HomeView>();

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

    private async void UpdateSecurityManagerCard()
    {
        if (_currentUserProvider.Profile.TwoFactorEnabled)
        {
            ShowTwoFactorEnabledState();
            return;
        }

        await ShowTwoFactorDisabledStateAsync();
    }

    private void ShowTwoFactorEnabledState()
    {
        TwoFactorEnabledContent.IsVisible = true;
        TwoFactorDisabledContent.IsVisible = false;
        DownloadBackupCodesButton.IsVisible = _currentUserProvider.TwoFactorBackupCodes?.Any() == true;
    }

    private async Task ShowTwoFactorDisabledStateAsync()
    {
        TwoFactorEnabledContent.IsVisible = false;
        TwoFactorDisabledContent.IsVisible = true;

        if (TwoFactorQRCodeImage.Source is not null && SetupKeyTextBlock.Text is not null)
            return;

        var setup = await _twoFactorAuthService.GetSetupInitAsync();
        TwoFactorQRCodeImage.Source = new Bitmap(new MemoryStream(setup.QrCodeData));
        SetupKeyTextBlock.Text = setup.SetupKey;
    }

    private async void EnableTwoFactorClick(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        => await _twoFactorAuthService.EnableAsync(SetupKeyTextBlock.Text ?? string.Empty);

    private async void DisableTwoFactorClick(object? sender, RoutedEventArgs e)
        => await _twoFactorAuthService.DisableAsync();

    private async void DisableTwoFactorCheckClick(object? sender, RoutedEventArgs e)
    {
        if (DisableTwoFactorButton.IsVisible) return;
        DisableTwoFactorButton.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(DisableTwoFactorButton);
    }

    private async void GenerateBackupCodesClick(object? sender, RoutedEventArgs e)
        => await _twoFactorAuthService.GenerateBackupCodesAsync();

    private async void DownloadBackupCodesClick(object? sender, RoutedEventArgs e)
    {
        var backupCodes = _currentUserProvider.TwoFactorBackupCodes;
        if (backupCodes?.Any() != true) return;

        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storageProvider) return;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Backup Codes",
            SuggestedFileName = "backup-codes.txt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Text File") { Patterns = new[] { "*.txt" } }
            }
        });

        if (file is null) return;

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(string.Join(Environment.NewLine, backupCodes));
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

    private async void UpdateActiveDevices()
    {
        DevicesPanel.Children.Clear();

        var tokenHash = Sha256Hasher.ComputeBase64(
            Convert.FromBase64String(await _refreshTokenProvider.GetAsync()));

        foreach (var device in _currentUserProvider.ActiveDevices)
            DevicesPanel.Children.Add(CreateDeviceCard(device, device.TokenHash == tokenHash));
    }

    private Control CreateDeviceCard(SessionDto device, bool isCurrentDevice)
    {
        var confirmButton = isCurrentDevice
            ? CreateConfirmButton(async () => await _sessionService.SignOutAsync())
            : CreateConfirmButton(async () => await ConfirmRevokeAsync(device.TokenHash));

        var defaultButton = isCurrentDevice
            ? CreateDefaultButton("Sign Out", confirmButton)
            : CreateDefaultButton("Revoke", confirmButton);

        var leftStack = new StackPanel { Spacing = 10 };

        if (isCurrentDevice)
        {
            leftStack.Children.Add(new TextBlock
            {
                Text = "This Device",
                Classes = { "deviceLabel" },
                Foreground = new SolidColorBrush(Color.Parse("#4CAF50"))
            });
        }

        leftStack.Children.Add(new ContentControl { Classes = { "deviceIp" }, Tag = device.IpAddress });

        var datesGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("* *") };
        var issuedAt = new ContentControl { Classes = { "deviceIssuedAt" }, Tag = DateTimeFormatter.ToOrdinalDate(device.IssuedAt) };
        var expiresAt = new ContentControl { Classes = { "deviceExpiresAt" }, Tag = DateTimeFormatter.ToOrdinalDate(device.ExpiresAt) };
        Grid.SetColumn(issuedAt, 0);
        Grid.SetColumn(expiresAt, 1);
        datesGrid.Children.Add(issuedAt);
        datesGrid.Children.Add(expiresAt);
        leftStack.Children.Add(datesGrid);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("* Auto") };
        Grid.SetColumn(defaultButton, 1);
        Grid.SetColumn(confirmButton, 1);
        grid.Children.Add(leftStack);
        grid.Children.Add(defaultButton);
        grid.Children.Add(confirmButton);

        return new Border { Classes = { "deviceCard" }, Child = grid };
    }

    private Button CreateDefaultButton(string label, Button confirmButton)
    {
        var button = new Button
        {
            Classes = { "DefaultDisable" },
            Content = new TextBlock
            {
                Text = label,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse("#FF5733")),
                FontSize = 12,
                FontWeight = FontWeight.Medium
            }
        };

        button.Click += async (_, _) =>
        {
            confirmButton.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(confirmButton);
            await Task.Delay(3000);
            await Animations.FadeOutAnimation.RunAsync(confirmButton);
            confirmButton.IsVisible = false;
        };

        return button;
    }

    private Button CreateConfirmButton(Func<Task> onClick)
    {
        var button = new Button
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

        button.Click += async (_, _) => await onClick();
        return button;
    }

    private async Task ConfirmRevokeAsync(string tokenHash)
        => await _tokenService.RevokeAsync(new SessionRevokeRequest { TokenHash = tokenHash }, CancellationToken.None);

    private async void LogOutAllDevicesClick(object? sender, RoutedEventArgs e)
    {
        ConfirmLogOutAllDevicesButton.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(ConfirmLogOutAllDevicesButton);
        await Task.Delay(3000);
        await Animations.FadeOutAnimation.RunAsync(ConfirmLogOutAllDevicesButton);
        ConfirmLogOutAllDevicesButton.IsVisible = false;
    }

    private async void ConfirmLogOutAllDevicesClick(object? sender, RoutedEventArgs e)
        => await _tokenService.RevokeAllAsync(new RevokeAllSessionsRequest(), CancellationToken.None);

    private void SetAccountBadges(UserRole role)
    {
        AccountBadgesStackPanel.Children.Clear();

        var badges = role switch
        {
            UserRole.Standard => new[] { "user" },
            UserRole.Administrator => new[] { "user", "admin" },
            UserRole.Founder => new[] { "user", "admin", "founder" },
            _ => Array.Empty<string>()
        };

        foreach (var badge in badges)
            AccountBadgesStackPanel.Children.Add(new Border
            {
                Classes = { "badge" },
                Child = new TextBlock { Classes = { "badgeText" }, Text = badge }
            });
    }

    private static (string bg, string fg) ActivityTypeColors(ActivityType type) => type switch
    {
        ActivityType.User => ("#E8F5E9", "#2E7D32"),
        ActivityType.System => ("#E3F2FD", "#1565C0"),
        ActivityType.Cron => ("#FFF8E1", "#F57F17"),
        _ => ("#EDE7DE", "#6D4C41")
    };

    private static string FormatDeviceCount(int count)
        => $"{count} {(count == 1 ? "Device" : "Devices")}";

    private static string FormatInvitationCount(int count)
        => $"{count} {(count == 1 ? "User" : "Users")}";
}