using Avalonia.Controls;
using Avalonia.VisualTree;
using MyrinaUI.Services;
using MyrinaUI.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyrinaUI.ViewModels {
    public class LogViewModel : ViewModelBase {
        public static LogViewModel LogView;

        private ObservableCollection<string> LogItems { get; } = new ObservableCollection<string>();

        private int _sIndex;
        public int SIndex {
            get { return _sIndex; }
            set { this.RaiseAndSetIfChanged(ref _sIndex, value); }
        }

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
            // 50 is arbitrary but...Behold the field in which I grow 
            // my fucks and see that it is barren.
            if (LogItems.Count > 50)
                LogItems.RemoveAt(0);

            var lines = s.Split('\n');
            foreach (var line in lines) {
                if (!string.IsNullOrWhiteSpace(line)) {
                    LogItems.Add($"{DateTime.Now.ToString("HH:mm tt")}: {line}");
                    SIndex = LogItems.Count - 1;
                }
            }
        }

        private GridLength _oldGridHeight;
        public void CollapseListBox() {
            var lv = ViewFinder.Get<LogView>();
            if (lv == null) return;

            var grid = lv.GetVisualAncestors().OfType<Grid>().FirstOrDefault();
            var row = Grid.GetRow(lv);

            _oldGridHeight = grid.RowDefinitions[row].Height;
            grid.RowDefinitions[row].Height = new GridLength(0, GridUnitType.Auto);

            ListBoxIsOpen = false;
        }

        public void ExpandListBox() {
            var lv = ViewFinder.Get<LogView>();
            if (lv == null) return;

            var grid = lv.GetVisualAncestors().OfType<Grid>().FirstOrDefault();
            var row = Grid.GetRow(lv);

            grid.RowDefinitions[row].Height = _oldGridHeight;

            ListBoxIsOpen = true;
        }
    }
}