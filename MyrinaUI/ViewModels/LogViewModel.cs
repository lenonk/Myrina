using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace MyrinaUI.ViewModels {
    public class LogViewModel : ViewModelBase {
        public static LogViewModel LogView;

        private ObservableCollection<string> LogItems { get; } = new ObservableCollection<string>();

        private bool _listBoxisOpen = true;
        public bool ListBoxIsOpen {
            get { return _listBoxisOpen; }
            set { this.RaiseAndSetIfChanged(ref _listBoxisOpen, value); }
        }

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

            var lines = s.Split('\n');
            foreach (var line in lines) {
                if (!string.IsNullOrWhiteSpace(line))
                    LogItems.Add($"{DateTime.Now.ToString("HH:mm tt")}: {line}");
            }
        }

        public void CollapseListBox() => ListBoxIsOpen = false;
        public void ExpandListBox() => ListBoxIsOpen = true;
    }
}