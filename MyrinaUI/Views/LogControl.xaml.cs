using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyrinaUI.UserControls {
    public class LogControl : UserControl {
        public LogControl() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
