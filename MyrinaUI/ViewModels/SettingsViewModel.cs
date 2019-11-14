using Amazon.EC2.Model;
using Avalonia.Controls;
using Avalonia.VisualTree;
using MyrinaUI.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MyrinaUI.ViewModels {
    public class SettingsViewModel : ViewModelBase {
        public static SettingsViewModel Settings;
        private Window MainWindow;

        const string configFile = "Myrina.cfg";

        #region Properties
        private string _accessKey;
        public string AccessKey {
            get { return _accessKey; }
            set { this.RaiseAndSetIfChanged(ref _accessKey, value); }
        }

        private string _secretKey;
        public string SecretKey {
            get { return _secretKey; }
            set { this.RaiseAndSetIfChanged(ref _secretKey, value); }
        }

        private string _defZone;
        public string DefZone {
            get { return _defZone; }
            set { this.RaiseAndSetIfChanged(ref _defZone, value); }
        }

        private string _defInstanceSize;
        public string DefInstanceSize {
            get { return _defInstanceSize; }
            set { this.RaiseAndSetIfChanged(ref _defInstanceSize, value); }
        }

        private string _defAmi;
        public string DefAmi {
            get { return _defAmi; }
            set { this.RaiseAndSetIfChanged(ref _defAmi, value); }
        }

        private string _defVpc;
        public string DefVpc {
            get { return _defVpc; }
            set { this.RaiseAndSetIfChanged(ref _defVpc, value); }
        }

        private ObservableCollection<Tag> _defTags = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> DefTags {
            get { return _defTags; }
            set { this.RaiseAndSetIfChanged(ref _defTags, value); }
        }
        #endregion

        public SettingsViewModel() { }

        public SettingsViewModel(Window parent) {
            MainWindow = parent;
            if (File.Exists(configFile)) { Read(configFile); }

            Settings = this;
        }

        public void AddTag() => DefTags.Add(new Tag() { Key = "", Value = "" });
        public void DeleteTag(Tag key) =>  DefTags.Remove(key);

        public void Show() {
            var sv = MainWindow.GetVisualDescendants()
                .OfType<SettingsView>()
                .FirstOrDefault(x => x.Name == "Settings");

            sv.Show();
        }

        public void Hide() {
            var sv = MainWindow.GetVisualDescendants()
                .OfType<SettingsView>()
                .FirstOrDefault(x => x.Name == "Settings");

            sv.Hide();
            Save("Myrina.cfg");
        }

        public void Save(string fname) {
            using (StreamWriter sw = new StreamWriter(fname)) {
                string s = JsonSerializer.Serialize(this, typeof(SettingsViewModel));
                sw.Write(s);
            }
        }

        public void Read(string fname) {
            using (StreamReader sw = new StreamReader(fname)) {
                var svm = JsonSerializer.Deserialize<SettingsViewModel>(sw.ReadToEnd());
                this.AccessKey = svm.AccessKey;
                this.SecretKey = svm.SecretKey;
                this.DefZone = svm.DefZone;
                this.DefInstanceSize = svm.DefInstanceSize;
                this.DefAmi = svm.DefAmi;
                this.DefVpc = svm.DefVpc;
                this.DefTags = svm.DefTags;
            }
        }
    }
}
