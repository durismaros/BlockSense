using BlockSense.Services;
using BlockSense.Views;
using CommunityToolkit.Mvvm.Input;

namespace BlockSense.ViewModels
{
    /// <summary>
    /// Represents the ViewModel for the Main screen of the application.
    /// Handles user navigation actions.
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        private readonly NavigationService _navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="navigationService">The navigation service responsible for switching application views.</param>
        public MainViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        /// <summary>
        /// Navigates the user to the Login view.
        /// </summary>
        [RelayCommand]
        private void Login()
        {
            _navigationService.NavigateTo<LoginView>();
        }

        /// <summary>
        /// Navigates the user to the Register view.
        /// </summary>
        [RelayCommand]
        private void Register()
        {
            _navigationService.NavigateTo<RegisterView>();
        }
    }
}
