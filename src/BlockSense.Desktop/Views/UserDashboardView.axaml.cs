using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.Enums.User;
using BlockSense.Desktop.Providers.Implementations;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;

namespace BlockSense.Desktop;

public partial class UserDashboardView : UserControl
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly NavigationManager _navigationManager;
    private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

    public UserDashboardView()
    {
        _currentUserProvider = App.ServiceProvider.GetRequiredService<ICurrentUserProvider>()
            ?? throw new ArgumentNullException(nameof(ICurrentUserProvider));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        _twoFactorSlidingPanel = MainWindow.Instance.TwoFactorSlidingPanel
            ?? throw new ArgumentNullException(nameof(MainWindow.Instance.TwoFactorSlidingPanel));

        InitializeComponent();

        UsernameTextBlock.Text =
            _currentUserProvider.Profile.Username;

        EmailTextBlock.Text =
            _currentUserProvider.Profile.Email;

        UserIdTextBlock.Text =
            _currentUserProvider.Profile.UserId.ToString();

        AddAccountBadges(_currentUserProvider.Profile.UserType);

        CreationDateTextBlock.Text =
            ToOrdinalDate(_currentUserProvider.Profile.CreatedAt);
        

        InvitedByTextBlock.Text =
            _currentUserProvider.Profile.InvitedBy;

        UpdatedAtTextBlock.Text =
            $"Updated: {ToOrdinalDate(_currentUserProvider.Profile.UpdatedAt)}";

        TwoFaStatusTextBlock.Text =
            _currentUserProvider.Profile.TwoFactorEnabled ? "Enabled" : "Disabled";

        UpdateTwoFaVisibility(_currentUserProvider.Profile.TwoFactorEnabled);

        ActiveDevicesTextBlock.Text =
            FormatDeviceCount(_currentUserProvider.ActiveDevices.Count);

        TotalInvitedUsersTextBlock.Text =
            FormatInvitationCount(_currentUserProvider.Invitations.Count);

        HomeButton.Click += ToHomeViewClick;

        ManageSecuritySettingsButton.Click += OpenSecurityManagerClick;
        CloseSecuriyManagerButton.Click += CloseSecurityManagerClick;

        ManageActiveDevicesButton.Click += OpenDeviceManagerClick;
        CloseDeviceManagerButton.Click += CloseDeviceManagerClick;
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

    private void AddAccountBadges(UserType userType)
    {
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

    private void UpdateTwoFaVisibility(bool twoFaEnabled)
    {
        TwoFaDisabledContent.IsVisible = !twoFaEnabled;
        TwoFaEnabledContent.IsVisible = twoFaEnabled;
    }

    private static string ToOrdinalDate(DateTime date)
        => $"{date.ToString("MMM", CultureInfo.InvariantCulture)} {date.Day.Ordinalize()}, {date:yyyy}";

    private static string FormatDeviceCount(int count)
        => $"{count} {(count == 1 ? "Device" : "Devices")}";

    private static string FormatInvitationCount(int count)
    => $"{count} {(count != 1 ? "Users" : "User")}";
}