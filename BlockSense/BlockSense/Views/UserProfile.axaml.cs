using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BlockSense.Client_Side;
using BlockSense.Server;
using BlockSense.Views;
using System.Globalization;
using System;
using BlockSense.Client.Utilities;
using System.Threading.Tasks;

namespace BlockSense;

public partial class UserProfile : UserControl
{
    private Bitmap _pfpBitmap = ProfilePictureHandler.ExistingPicture();

    public UserProfile()
    {
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
        if (User.Uid != 0)
        {
            ProfilePictureImage.Source = _pfpBitmap;
            UsernameTextBlock.Text = User.Username;
            UidTextBlock.Text = User.Uid.ToString();
            EmailTextBlock.Text = User.Email;
            switch (User.Type)
            {
                case User.UserType.User:
                    AccountBadgesPanel.Children.Add(userBadge);
                    break;

                case User.UserType.Admin:
                    AccountBadgesPanel.Children.Add(userBadge);
                    AccountBadgesPanel.Children.Add(adminBadge);
                    break;
            }
            CreationDateTextBlock.Text = SystemUtils.DateTransform(User.CreatedAt);
            InvitationUserTextBlock.Text = User.InvitingUser;
            LastUpdateTextBlock.Text = SystemUtils.DateTransform(User.UpdatedAt);
            int invitedUsers = User.AdditionalInformation.InvitedUsers;
            UsersInvitedTextBlock.Text = $"{invitedUsers.ToString()} {(invitedUsers > 1 ? "Users" : "User")}";
            int activeDevices = User.AdditionalInformation.ActiveDevices;
            ActiveDevicesTextBlock.Text = $"{activeDevices.ToString()} {(activeDevices > 1 ? "Devices" : "Device")}";
        }

    }

    private async void HomeClick(object sender, RoutedEventArgs e)
    {
        await MainWindow.SwitchView(new Welcome());
    }

    private async void PfpUploadClick(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(sender as Avalonia.Visual).Properties.IsLeftButtonPressed)
        {
            var parentWindow = this.VisualRoot as Window;
            await ProfilePictureHandler.UploadFile(parentWindow!);
            ProfilePictureImage.Source = ProfilePictureHandler.ExistingPicture();
        }
    }

    private void SetDefaultClick(object sender, RoutedEventArgs e)
    {
        ProfilePictureImage.Source = ProfilePictureHandler.setDefaultPfp();
    }

    private async void LogoutClick(object sender, RoutedEventArgs e)
    {
        await User.Logout();
        await MainWindow.SwitchView(new MainView());
    }
}