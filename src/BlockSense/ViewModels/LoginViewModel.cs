using BlockSense.Models.Login;
using BlockSense.Services;
using BlockSense.Utilities.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BlockSense.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly UserService _userService;
        private readonly NavigationService _navigationService;
        private readonly TwoFactorSlidingPanel _twoFactorPanel;
        private readonly AsyncDebouncer _debouncer = new();

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private bool _hasMessage;

        public LoginViewModel(
        UserService userService,
        NavigationService navigationService,
        TwoFactorSlidingPanel twoFactorPanel)
        {
            _userService = userService;
            _navigationService = navigationService;
            _twoFactorPanel = twoFactorPanel;

            _twoFactorPanel.CodeSubmitted += OnTwoFactorSubmitted;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            await _debouncer.TryExecuteAsync("login", async () =>
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ShowMessage("Looks like you missed a required field");
                    return;
                }

                var response = await _userService.Login(new LoginRequest(Username, Password));

                if (response == null)
                    return;

                ShowMessage(response.Message);

                if (response.TwoFactorRequired)
                {
                    await _twoFactorPanel.ShowPanel(TwoFactorSlidingPanel.TwoFactorMode.Verify);
                    return;
                }

                if (!response.Success)
                    return;

                await Task.Delay(1500);
                _navigationService.NavigateTo<WelcomeViewModel>();
            });
        }

        [RelayCommand]
        private void GoHome()
        {
            _navigationService.NavigateTo<MainViewModel>();
        }

        [RelayCommand]
        private void ResetPassword()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.google.com/",
                UseShellExecute = true
            });
        }

        private async void OnTwoFactorSubmitted(object? sender, TwoFactorCodeEventArgs e)
        {
            await _debouncer.TryExecuteAsync("2fa", async () =>
            {
                var response = await _userService.Login(
                    new LoginRequest(Username, Password, e.Code));

                if (response == null || !response.Success)
                {
                    await _twoFactorPanel.ShowError();
                    return;
                }

                ShowMessage(response.Message);
                await _twoFactorPanel.ShowSuccessState();

                await Task.Delay(1000);
                _navigationService.NavigateTo<WelcomeViewModel>();
            });
        }

        private void ShowMessage(string text)
        {
            Message = text;
            HasMessage = true;
        }
    }
}
