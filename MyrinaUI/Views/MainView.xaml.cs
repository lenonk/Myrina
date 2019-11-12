using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Linq;

namespace MyrinaUI.Views {
    public class MainView : UserControl {
        public MainView() {
            this.InitializeComponent();

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

        public void OnRowClicked(object sender, SelectionChangedEventArgs e) {
            e.Handled = true;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
