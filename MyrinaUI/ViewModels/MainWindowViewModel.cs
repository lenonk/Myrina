using Avalonia.Controls;

namespace MyrinaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase {

        public MainViewModel MainView { get; }

        public MainWindowViewModel(Window parent) {
            MainView = new MainViewModel(parent);
        }
    }
}
