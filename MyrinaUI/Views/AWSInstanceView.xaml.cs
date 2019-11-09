using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyrinaUI.Views {
    public class AWSInstanceView : UserControl {
        public AWSInstanceView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
