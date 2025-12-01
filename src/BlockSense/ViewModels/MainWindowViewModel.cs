using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlockSense.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? _currentView;

        public MainWindowViewModel()
        {
            // Initial view
            CurrentView = new PlainViewModel();
        }

        // Example command to change views
        [RelayCommand]
        public void ShowPlainView()
        {
            CurrentView = new PlainViewModel();
        }
    }
}
