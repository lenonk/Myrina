using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MsgBox;

namespace MyrinaUI.Views
{
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
