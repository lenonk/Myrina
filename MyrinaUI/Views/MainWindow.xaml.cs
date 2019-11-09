using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MsgBox;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace MyrinaUI.Views
{
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            var grid = this.FindControl<DataGrid>("instanceGrid");
            grid.AddHandler(
                InputElement.PointerPressedEvent,
                (s, e) => {
                    if (e.MouseButton == MouseButton.Right) {
                        var row = ((IControl)e.Source).GetSelfAndVisualAncestors()
                            .OfType<DataGridRow>()
                            .FirstOrDefault();

                        if (row != null) {
                            grid.SelectedIndex = row.GetIndex();
                        }
                    }
                },
                handledEventsToo: true);

        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnRowClicked(object sender, SelectionChangedEventArgs e) {
            e.Handled = true;
        }
    }
}
