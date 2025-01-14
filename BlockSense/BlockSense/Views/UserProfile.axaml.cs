using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlockSense.DB;
using BlockSense.Views;
using System.Threading.Tasks;

namespace BlockSense;

public partial class UserProfile : UserControl
{
    public UserProfile()
    {
        InitializeComponent();

        uid_span.Inlines.Add(User.Uid.ToString());
        username_span.Inlines.Add(User.Username);
        email_span.Inlines.Add(User.Email);
        type_span.Inlines.Add(User.Type);
        creationDate_span.Inlines.Add(User.CreationDate);
        invitingUser_span.Inlines.Add(User.InvitingUser);
    }

    private void HomeClick(object sender, RoutedEventArgs e)
    {
        Content = new Welcome();
    }

    private async void LogoutClick(object sender, RoutedEventArgs e)
    {
        await User.Logout();
        Content = new MainView();
    }
}