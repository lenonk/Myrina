using Avalonia.Controls;
using Avalonia.VisualTree;
using System.Linq;

namespace MyrinaUI.Services {
    public class ViewFinder {
        private static Window _mainWindow;

        private static ViewFinder _current;
        public static ViewFinder Current {
            get {
                if (_mainWindow == null)
                    return null;

                return _current ?? (_current = new ViewFinder());
            }
        }

        public static void Initialize(Window p) {
            _mainWindow = p;
        }

        public static T Get<T>() {
            return _mainWindow.GetVisualDescendants()
                .OfType<T>()
                .FirstOrDefault();
        }
    }
}
