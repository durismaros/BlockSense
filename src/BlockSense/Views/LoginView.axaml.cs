using Avalonia.Controls;
using Avalonia.Input;
using BlockSense.ViewModels;

namespace BlockSense.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        this.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
        {
            vm.LoginCommand.Execute(null);
        }
    }
}
