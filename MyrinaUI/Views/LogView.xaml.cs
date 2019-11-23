using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyrinaUI.Views {
    public class LogView : UserControl {
        public LogView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
