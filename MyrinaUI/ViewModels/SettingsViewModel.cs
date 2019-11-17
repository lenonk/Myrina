using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.ObjectModel;
using ReactiveUI;
using MyrinaUI.Views;
using Amazon.EC2.Model;
using MyrinaUI.Services;

namespace MyrinaUI.ViewModels {
    public class SettingsViewModel : ViewModelBase {

        #region Properties
        private string _accessKey = Settings.Current.AccessKey;
        public string AccessKey {
            get { return _accessKey; }
            set { this.RaiseAndSetIfChanged(ref _accessKey, value); }
        }

        private string _secretKey = Settings.Current.SecretKey;
        public string SecretKey {
            get { return _secretKey; }
            set { this.RaiseAndSetIfChanged(ref _secretKey, value); }
        }

        private string _zone = Settings.Current.Zone;
        public string Zone {
            get { return _zone; }
            set { this.RaiseAndSetIfChanged(ref _zone, value); }
        }

        private string _instanceType = Settings.Current.InstanceType;
        public string InstanceType {
            get { return _instanceType; }
            set { this.RaiseAndSetIfChanged(ref _instanceType, value); }
        }

        private string _image = Settings.Current.Image;
        public string Image {
            get { return _image; }
            set { this.RaiseAndSetIfChanged(ref _image, value); }
        }

        private string _vpc = Settings.Current.Vpc;
        public string Vpc {
            get { return _vpc; }
            set { this.RaiseAndSetIfChanged(ref _vpc, value); }
        }

        private ObservableCollection<Tag> _tags = Settings.Current.Tags;
        public ObservableCollection<Tag> Tags {
            get { return _tags; }
            set { this.RaiseAndSetIfChanged(ref _tags, value); }
        }
        #endregion

        public SettingsViewModel() {
            this.WhenAnyValue(x => x.AccessKey)
                .Subscribe(x => EventSystem.Publish(new AccessKeyChanged() { value = x }));
            this.WhenAnyValue(x => x.SecretKey)
                .Subscribe(x => EventSystem.Publish(new SecretKeyChanged() { value = x }));
            this.WhenAnyValue(x => x.Zone)
                .Subscribe(x => EventSystem.Publish(new ZoneChanged() { value = x }));
            this.WhenAnyValue(x => x.InstanceType)
                .Subscribe(x => EventSystem.Publish(new InstanceTypeChanged() { value = x }));
            this.WhenAnyValue(x => x.Image)
                .Subscribe(x => EventSystem.Publish(new ImageChanged() { value = x }));
            this.WhenAnyValue(x => x.Vpc)
                .Subscribe(x => EventSystem.Publish(new VpcChanged() { value = x }));
            this.WhenAnyValue(x => x.Tags)
                .Subscribe(x => EventSystem.Publish(new TagsChanged() { value = x }));
        }

        public void AddTag() => Tags.Add(new Tag() { Key = "", Value = "" });
        public void DeleteTag(Tag key) =>  Tags.Remove(key);

        public void Show() {
            var sv = ViewFinder.Get<SettingsView>();
            sv.Show();
        }

        public void Hide() {
            var sv = ViewFinder.Get<SettingsView>();
            Settings.Current.Save();
            sv.Hide();
        }
    }
}
