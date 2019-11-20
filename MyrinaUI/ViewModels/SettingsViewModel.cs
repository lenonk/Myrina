using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using MyrinaUI.Views;
using Amazon.EC2.Model;
using MyrinaUI.Services;
using Amazon.EC2;
using System.Reflection;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Reactive;

namespace MyrinaUI.ViewModels {
    public class SettingsViewModel : ViewModelBase {
        public ObservableCollection<string> EC2AvailabilityZones { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> EC2InstanceTypes { get; } = new ObservableCollection<string>();
        public ObservableCollection<Image> EC2Images { get; } = new ObservableCollection<Image>();
        public ObservableCollection<Vpc> EC2Vpcs { get; set; } = new ObservableCollection<Vpc>();
        public ObservableCollection<KeyPairInfo> EC2KeyPairs { get; set; } = new ObservableCollection<KeyPairInfo>();
        public ObservableCollection<Tag> EC2Tags { get; } = new ObservableCollection<Tag>();

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

        private string _sZone = Settings.Current.Zone; 
        public string SZone {
            get { return _sZone; }
            set { this.RaiseAndSetIfChanged(ref _sZone, value); }
        }

        private string _sInstanceType = Settings.Current.InstanceType;
        public string SInstanceType {
            get { return _sInstanceType; }
            set { this.RaiseAndSetIfChanged(ref _sInstanceType, value); }
        }

        private Image _sImage;
        public Image SImage {
            get { return _sImage; }
            set { this.RaiseAndSetIfChanged(ref _sImage, value); }
        }

        private Vpc _sVpc;
        public Vpc SVpc {
            get { return _sVpc; }
            set { this.RaiseAndSetIfChanged(ref _sVpc, value); }
        }

        private KeyPairInfo _sKey;
        public KeyPairInfo SKey {
            get { return _sKey; }
            set { this.RaiseAndSetIfChanged(ref _sKey, value); }
        }

        private ObservableCollection<Tag> _tags = Settings.Current.Tags;
        public ObservableCollection<Tag> Tags {
            get { return _tags; }
            set { this.RaiseAndSetIfChanged(ref _tags, value); }
        }
        #endregion

        public SettingsViewModel() {
            const int AccessKeyLength = 20;
            const int SecretKeyLength = 40;

            RefreshEC2AllData();

            this.WhenAnyValue(x => x.AccessKey).Skip(1)
                .Where(x => x != null)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(x => EventSystem.Publish(new AccessKeyChanged() { value = x }));

            this.WhenAnyValue(x => x.SecretKey).Skip(1)
                .Where(x => x != null)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(x => EventSystem.Publish(new SecretKeyChanged() { value = x }));

            this.WhenAnyValue(x => x.SZone).Skip(1)
                .Where(x => x != null)
                .Subscribe(x => EventSystem.Publish(new ZoneChanged() { value = x }));
            this.WhenAnyValue(x => x.SInstanceType).Skip(1)
                .Where(x => x != null)
                .Subscribe(x => EventSystem.Publish(new InstanceTypeChanged() { value = x }));
            this.WhenAnyValue(x => x.SImage).Skip(1)
                .Where(x => x != null)
                .Subscribe(x => EventSystem.Publish(new ImageChanged() { value = x.ImageId}));
            this.WhenAnyValue(x => x.SVpc).Skip(1)
                .Where(x => x != null)
                .Subscribe(x => EventSystem.Publish(new VpcChanged() { value = x.VpcId }));
            this.WhenAnyValue(x => x.SKey).Skip(1)
                .Where(x => x != null)
                .Subscribe(x => EventSystem.Publish(new KeyPairChanged() { value = x.KeyName }));
            this.WhenAnyValue(x => x.Tags).Skip(1)
                .Where(x => x != null)
                .Subscribe(x => EventSystem.Publish(new TagsChanged() { value = x }));

            this.WhenAnyValue(x => x.AccessKey, y => y.SecretKey)
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Item1) &&
                    !string.IsNullOrWhiteSpace(x.Item2) &&
                    x.Item1.Length >= AccessKeyLength &&
                    x.Item2.Length >= SecretKeyLength)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => RefreshEC2AllData());
        }

        public void AddTag() => Tags.Add(new Tag() { Key = "", Value = "" });
        public void DeleteTag(Tag key) =>  Tags.Remove(key);

        public void Hide() {
            var sv = ViewFinder.Get<SettingsView>();
            Settings.Current.Save();
            sv.Hide();
        }

        // Data refresh methods
        private enum AmazonRefreshCode {
            Zones,
            Types,
            Subnets,
            SecurityGroups,
            Vpcs,
            Images,
            KeyPairs,
            All
        }

        private async void RefreshAmazonData(AmazonRefreshCode code) {
            try {
                if (code == AmazonRefreshCode.Vpcs || code == AmazonRefreshCode.All) {
                    await EC2Service.Instance.GetEC2Vpcs(EC2Vpcs)
                        .ContinueWith(_ => SVpc = SettingsFirstOrDefault(Settings.Current.Vpc, EC2Vpcs, "VpcId"));
                }
                if (code == AmazonRefreshCode.Zones || code == AmazonRefreshCode.All) {
                    await EC2Service.Instance.GetEC2AvailabilityZones(EC2AvailabilityZones)
                        .ContinueWith(_ => SZone = SettingsFirstOrDefault(Settings.Current.Zone, EC2AvailabilityZones));
                }
                if (code == AmazonRefreshCode.Types || code == AmazonRefreshCode.All) {
                    await EC2Service.Instance.GetEC2InstanceTypes(EC2InstanceTypes)
                        .ContinueWith(_ => SInstanceType = SettingsFirstOrDefault(Settings.Current.InstanceType, EC2InstanceTypes));
                }
                if (code == AmazonRefreshCode.KeyPairs || code == AmazonRefreshCode.All) {
                    await EC2Service.Instance.GetEC2KeyPairs(EC2KeyPairs)
                        .ContinueWith(_ => SKey = SettingsFirstOrDefault(Settings.Current.KeyPair, EC2KeyPairs, "KeyName"));
                }
                if (code == AmazonRefreshCode.Images || code == AmazonRefreshCode.All) {
                    // TODO: Pull these from the api
                    //await EC2Utility.GetEC2Images(EC2Images);
                    EC2Images.Clear();
                    EC2Images.Add(new Image { Name = "TMC CDIA Development", ImageId = "ami-4212963d" });
                    EC2Images.Add(new Image { Name = "TMC CDIA Integration", ImageId = "ami-c0e725bd" });
                    EC2Images.Add(new Image { Name = "TMC CDIA Master", ImageId = "ami-69dc1e14" });
                    EC2Images.Add(new Image { Name = "LM3 CDIA Integration", ImageId = "ami-03542d7c" });
                    SImage = SettingsFirstOrDefault(Settings.Current.Image, EC2Images, "ImageId");
                }
            }
            catch (AmazonEC2Exception e) {
                LogViewModel.LogView.Log(e.Message);
            }
        }

        public void RefreshEC2AvailibilityZones() => RefreshAmazonData(AmazonRefreshCode.Zones);
        public void RefreshEC2InstanceTypes() => RefreshAmazonData(AmazonRefreshCode.Types);
        public void RefreshEC2Vpcs() => RefreshAmazonData(AmazonRefreshCode.Vpcs);
        public void RefreshEC2Images() => RefreshAmazonData(AmazonRefreshCode.Images);
        public void RefreshKeyPairInfo() => RefreshAmazonData(AmazonRefreshCode.KeyPairs);
        public void RefreshEC2AllData() => RefreshAmazonData(AmazonRefreshCode.All);

        // Private helpers
        private T SettingsFirstOrDefault<T>(string value, ObservableCollection<T> col, string property = null) {
            if (value != null && value != string.Empty) {
                foreach (T x in col) {
                    if (x as string == value)
                        return x;

                    if (property != null) {
                        PropertyInfo pi = x.GetType().GetProperty(property);
                        if (pi != null && pi.GetValue(x) as string == value)
                            return x;
                    }
                }
            }

            if (col != null && col.Count > 0)
                return col[0];

            return default(T);
        }
    }
}
