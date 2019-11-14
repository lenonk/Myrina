using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

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

            var button = this.FindControl<Button>("CollapseButton");
            button.Click += (o, e) => {
                ToggleListBox();
            };
        }

        void ToggleListBox() {
            var lb = this.FindControl<ListBox>("LogListBox");
            if (lb.Height > 0)
                lb.Height = 0;
            else
                lb.Height = 100;
        }
    }
}
