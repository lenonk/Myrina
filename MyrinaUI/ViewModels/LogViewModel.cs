using Avalonia.Controls;
using Avalonia.VisualTree;
using MyrinaUI.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyrinaUI.ViewModels {
    public class LogViewModel : ViewModelBase {
        public static LogViewModel LogView;

        private ObservableCollection<string> LogItems { get; } = new ObservableCollection<string>();

        public LogViewModel() {
            LogView = this;

            Log("Welcome to Myrina!");
            Log("Initializing all the things...");
        }

        public void Log(string s) {
            // 500 is arbitrary but...Behold the field in which I grow 
            // my fucks and see that it is barren.
            if (LogItems.Count > 500)
                LogItems.RemoveAt(0);

            LogItems.Add($"{DateTime.Now.ToString("HH:mm tt")}: {s}");
        }
    }
}