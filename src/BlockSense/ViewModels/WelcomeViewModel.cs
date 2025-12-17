using BlockSense.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.ViewModels
{
    /// <summary>
    /// WelcomeViewModel for the landing or session-start page.
    /// Handles navigation and initialization logic.
    /// </summary>
    public partial class WelcomeViewModel : ViewModelBase
    {
        private readonly NavigationService _navigationService;

        public event Action? RequestFadeIn;

        public WelcomeViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            RequestFadeIn?.Invoke();
        }

        [RelayCommand]
        private void OpenProfile()
        {
            _navigationService.NavigateTo<UserProfileViewModel>();
        }

        [RelayCommand]
        private void OpenWallet()
        {
            _navigationService.NavigateTo<PinEntryViewModel>();
        }
    }
}
