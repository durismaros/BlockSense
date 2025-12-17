using CommunityToolkit.Mvvm.ComponentModel;

namespace BlockSense.ViewModels
{
    /// <summary>
    /// Root ViewModel of the application. Exposes CurrentViewModel for display.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? _currentViewModel;

        public MainWindowViewModel()
        {
            // Initial landing page
            _currentViewModel = new PlainViewModel();
        }

        /// <summary>
        /// Switches the visible ViewModel.
        /// </summary>
        public void NavigateTo(ViewModelBase viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}
