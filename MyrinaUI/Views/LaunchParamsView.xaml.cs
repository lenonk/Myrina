using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyrinaUI.Views {
    public class LaunchParamsView : UserControl {
        public LaunchParamsView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
