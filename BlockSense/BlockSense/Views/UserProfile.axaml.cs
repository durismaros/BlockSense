using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlockSense.Client_Side;
using BlockSense.DB;
using BlockSense.Views;
using System.Xml;

namespace BlockSense;

public partial class UserProfile : UserControl
{
    public UserProfile()
    {
        InitializeComponent();
        if (User.Uid != "0")
        {
            uid_span.Inlines.Add(User.Uid);
            username_span.Inlines.Add(User.Username);
            email_span.Inlines.Add(User.Email);
            type_span.Inlines.Add(User.Type);
            creationDate_span.Inlines.Add(User.CreationDate);
            invitingUser_span.Inlines.Add(User.InvitingUser);
            ProfilePicture.Source = ProfilePictureHandler.PictureBitmap();
        }

    }
    private void HomeClick(object sender, RoutedEventArgs e)
    {
        Content = new Welcome();
    }

    private async void PfpUploadClick(object sender, RoutedEventArgs e)
    {
        var parentWindow = this.VisualRoot as Window;
        await ProfilePictureHandler.UploadFile(parentWindow);
        ProfilePicture.Source = ProfilePictureHandler.PictureBitmap();
    }

    private async void LogoutClick(object sender, RoutedEventArgs e)
    {
        await User.Logout();
        Content = new MainView();
    }
}