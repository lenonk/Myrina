using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;

namespace MyrinaUI.Views {
    public class LogView : UserControl {
        public LogView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            var lb = this.FindControl<ListBox>("LogListBox");
            lb.PropertyChanged += (o, e) => {
                lb.SelectedIndex = lb.ItemCount - 1;
            };
        }
    }
}
