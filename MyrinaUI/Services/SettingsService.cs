using Amazon.EC2.Model;
using MyrinaUI.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace MyrinaUI.Services {
    public sealed class Settings {
        private string configFile = "Myrina.cfg";

        private static readonly Lazy<Settings> lazy = 
            new Lazy<Settings>(() => new Settings());

        public static Settings Current { get { return lazy.Value; } }

        private Settings() {
            if (File.Exists(configFile)) { ReadConfig(); }

            // Suvscribe to config changes from SettingsView
            EventSystem.Subscribe<AccessKeyChanged>((x) => { AccessKey = x.value; });
            EventSystem.Subscribe<SecretKeyChanged>((x) => { SecretKey = x.value; });
            EventSystem.Subscribe<ZoneChanged>((x) => { Zone = x.value; });
            EventSystem.Subscribe<InstanceTypeChanged>((x) => { InstanceType = x.value; });
            EventSystem.Subscribe<ImageChanged>((x) => { Image = x.value; });
            EventSystem.Subscribe<VpcChanged>((x) => { Vpc = x.value; });
            EventSystem.Subscribe<TagsChanged>((x) => { Tags = x.value; });
            EventSystem.Subscribe<KeyPairChanged>((x) => { KeyPair = x.value; });
        }

        private class SettingsProperties {
            public string AccessKey { get; set; }
            public string SecretKey { get; set; }
            public string Zone { get; set; }
            public string InstanceType { get; set; }
            public string Image { get; set; }
            public string Vpc { get; set; }
            public string KeyPair { get; set; }
            public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        }

        private SettingsProperties _settings = new SettingsProperties();

        public string AccessKey {
            get { return _settings.AccessKey; }
            private set { _settings.AccessKey = value; }
        }
        public string SecretKey {
            get { return _settings.SecretKey; } 
            private set { _settings.SecretKey = value; }
        }
        public string Zone { 
            get { return _settings.Zone; }
            private set { _settings.Zone = value; }
        }
        public string InstanceType { 
            get { return _settings.InstanceType; }
            private set { _settings.InstanceType = value; }
        }
        public string Image {
            get { return _settings.Image; } 
            private set { _settings.Image = value; }
        }
        public string Vpc { 
            get { return _settings.Vpc; }
            private set { _settings.Vpc = value; }
        }
        public string KeyPair { 
            get { return _settings.KeyPair; }
            private set { _settings.KeyPair = value; }
        }
        public ObservableCollection<Tag> Tags {
            get { return _settings.Tags; } 
            private set { _settings.Tags = value; }
        }

        public void Save() {
            using (StreamWriter sw = new StreamWriter(configFile)) {
                string s = JsonSerializer.Serialize(_settings, typeof(SettingsProperties));
                sw.Write(s);
            }

            EventSystem.Publish(new SettingsChanged());
        }

        public void ReadConfig() {
            try {
                using (StreamReader sw = new StreamReader(configFile)) {
                    _settings = JsonSerializer.Deserialize<SettingsProperties>(sw.ReadToEnd());
                }
            }
            catch (JsonException e) {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
