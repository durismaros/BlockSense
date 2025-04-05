using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BlockSense.Client;
using BlockSense.Client_Side;
using BlockSense.Server;
using BlockSense.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace BlockSense;

public partial class UserProfile : UserControl
{
    private Bitmap _pfpBitmap = ProfilePictureHandler.ExistingPicture();
    public UserProfile()
    {
        InitializeComponent();
        if (User.Uid != null)
        {
            uid_span.Inlines.Add(User.Uid);
            username_span.Inlines.Add(User.Username);
            email_span.Inlines.Add(User.Email);
            type_span.Inlines.Add(User.Type);
            creationDate_span.Inlines.Add(User.CreationDate);
            invitingUser_span.Inlines.Add(User.InvitingUser);
            ProfilePicture.Source = _pfpBitmap;
        }

    }

    private void HomeClick(object sender, RoutedEventArgs e)
    {
        Animations.AnimateTransition(this, new Welcome());
    }

    private async void PfpUploadClick(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(sender as Avalonia.Visual).Properties.IsLeftButtonPressed)
        {
            var parentWindow = this.VisualRoot as Window;
            await ProfilePictureHandler.UploadFile(parentWindow!);
            ProfilePicture.Source = ProfilePictureHandler.ExistingPicture();
        }
    }

    private void SetDefaultClick(object sender, RoutedEventArgs e)
    {
        ProfilePicture.Source = ProfilePictureHandler.setDefaultPfp();
    }

    private async void LogoutClick(object sender, RoutedEventArgs e)
    {
        await User.Logout();
        Animations.AnimateTransition(this, new MainView());
    }
}