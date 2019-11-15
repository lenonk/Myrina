using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Linq;

namespace MyrinaUI.Views {
    public class DataGridView : UserControl {
        public DataGridView() {
            this.InitializeComponent();

            var grid = this.FindControl<DataGrid>("instanceGrid");
            grid.AddHandler(
                InputElement.PointerPressedEvent,
                (s, e) => {
                    if (e.MouseButton == MouseButton.Right) {
                        var row = ((IControl)e.Source).GetSelfAndVisualAncestors()
                            .OfType<DataGridRow>()
                            .FirstOrDefault();

                        if (row != null && !grid.SelectedItems.Contains(row.DataContext)) {
                            grid.SelectedIndex = row.GetIndex();
                        }
                    }
                },
                handledEventsToo: true);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
