using System;
using System.Collections.ObjectModel;
using MyrinaUI.Models;
using ReactiveUI;
using MsgBox;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Amazon.EC2;
using Avalonia.Threading;
using Amazon.EC2.Model;
using System.Reactive.Linq;
using System.Reflection;

namespace MyrinaUI.ViewModels {
    using EC2Image = Amazon.EC2.Model.Image;

    public class MainViewModel : ViewModelBase {

        // Public class members
        public static Window MainWindow;
        public ObservableCollection<Instance> EC2Instances { get; } = new ObservableCollection<Instance>();
        public ObservableCollection<string> EC2InstanceTypes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> EC2AvailabilityZones { get; } = new ObservableCollection<string>();
        public ObservableCollection<EC2Image> EC2Images { get; } = new ObservableCollection<EC2Image>();
        public ObservableCollection<Subnet> EC2Subnets { get; private set; } = new ObservableCollection<Subnet>();
        public ObservableCollection<SecurityGroup> ActiveSecurityGroups { get; } = new ObservableCollection<SecurityGroup>();
        public ObservableCollection<SecurityGroup> EC2SecurityGroups { get; set; } = new ObservableCollection<SecurityGroup>();
        public ObservableCollection<Vpc> EC2Vpcs { get; set; } = new ObservableCollection<Vpc>();
        public ObservableCollection<KeyPairInfo> EC2KeyPairs { get; set; } = new ObservableCollection<KeyPairInfo>();

        // Private class members
        private DispatcherTimer _refreshTimer = new DispatcherTimer();
        private ObservableCollection<Tag> _ec2Tags = new ObservableCollection<Tag>();

        // Properties
        #region Properties
        private Instance _sInstance;
        public Instance SInstance {
            get { return _sInstance; }
            set { this.RaiseAndSetIfChanged(ref _sInstance, value); }
        }

        private string _sInstanceType;
        public string SInstanceType {
            get { return _sInstanceType; }
            set { this.RaiseAndSetIfChanged(ref _sInstanceType, value); }
        }

        private string _sAvailabilityZone;
        public string SAvailabilityZone {
            get { return _sAvailabilityZone; }
            set { this.RaiseAndSetIfChanged(ref _sAvailabilityZone, value); }
        }

        private EC2Image _sImage;
        public EC2Image SImage {
            get { return _sImage; }
            set { this.RaiseAndSetIfChanged(ref _sImage, value); }
        }

        private Subnet _sSubnet;
        public Subnet SSubnet {
            get { return _sSubnet; }
            set { this.RaiseAndSetIfChanged(ref _sSubnet, value); }
        }

        private KeyPairInfo _sKey;
        public KeyPairInfo SKey {
            get { return _sKey; }
            set { this.RaiseAndSetIfChanged(ref _sKey, value); }
        }

        private SecurityGroup _sSecurityGroup;
        public SecurityGroup SSecurityGroup {
            get { return _sSecurityGroup; }
            set { this.RaiseAndSetIfChanged(ref _sSecurityGroup, value); }
        }

        private Vpc _sVpc;
        public Vpc SVpc {
            get { return _sVpc; }
            set { this.RaiseAndSetIfChanged(ref _sVpc, value); }
        }

        private SettingsViewModel _settingsView;
        public SettingsViewModel SettingsView {
            get { return _settingsView; }
            set { this.RaiseAndSetIfChanged(ref _settingsView, value); }
        }

        public ObservableCollection<Tag> EC2Tags {
            get { return _ec2Tags; }
            set { this.RaiseAndSetIfChanged(ref _ec2Tags, value); }
        }

        private bool _usePublicIp = true;
        public bool UsePublicIp {
            get { return _usePublicIp; }
            set { this.RaiseAndSetIfChanged(ref _usePublicIp, value); }
        }

        private int _startNumber = 1;
        public int StartNumber {
            get { return _startNumber; }
            set { this.RaiseAndSetIfChanged(ref _startNumber, value); }
        }
        #endregion

        // Constructor
        public MainViewModel(Window parent) {
            MainWindow = parent;
            SettingsView = new SettingsViewModel(parent);

            Initialize();
        }

        // Methods
        public void AddSecurityGroup(SecurityGroup group) => ActiveSecurityGroups.Add(group);
        public void DeleteSecurityGroup(SecurityGroup group) => ActiveSecurityGroups.Remove(group);
        public void AddTag() => EC2Tags.Add(new Tag() { Key = "", Value = "" });
        public void DeleteTag(Tag key) => EC2Tags.Remove(key);
        public void ShowSettings() => SettingsView.Show();

        public void Initialize() {

            RefreshEC2AllData();

            this.WhenAnyValue(x => x.SVpc).Subscribe((x) => { 
                RefreshEC2Subnets(); 
                RefreshEC2SecurityGroups(); 
            });

            _refreshTimer.Interval = TimeSpan.FromSeconds(30);
            _refreshTimer.Tick += (sender, e) => { RefreshEC2Instances(); };
            _refreshTimer.Start();
        }

        // Data refresh methods
        private enum AmazonRefreshCode {
            Instances,
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
                if (code == AmazonRefreshCode.Instances || code == AmazonRefreshCode.All) {
                    Instance si = SInstance;
                    await EC2Utility.GetEC2Instances(EC2Instances)
                        .ContinueWith(_ => ResetSelectedInstance(si));
                }
                if (code == AmazonRefreshCode.Vpcs || code == AmazonRefreshCode.All) {
                    await EC2Utility.GetEC2Vpcs(EC2Vpcs)
                        .ContinueWith(_ => SVpc = SettingsFirstOrDefault(SettingsView.DefVpc, EC2Vpcs, "VpcId"));
                }
                if (code == AmazonRefreshCode.Zones || code == AmazonRefreshCode.All) {
                    await EC2Utility.GetEC2AvailabilityZones(EC2AvailabilityZones)
                        .ContinueWith(_ => SAvailabilityZone = SettingsFirstOrDefault(SettingsView.DefZone, EC2AvailabilityZones));
                }
                if (code == AmazonRefreshCode.Types || code == AmazonRefreshCode.All) {
                    await EC2Utility.GetEC2InstanceTypes(EC2InstanceTypes)
                        .ContinueWith(_ => SInstanceType = SettingsFirstOrDefault(SettingsView.DefInstanceSize, EC2InstanceTypes));
                }
                if (code == AmazonRefreshCode.Subnets || code == AmazonRefreshCode.All) {
                    await EC2Utility.GetEC2Subnets(EC2Subnets, SVpc)
                        .ContinueWith(_ => SSubnet = SettingsFirstOrDefault("", EC2Subnets));
                }
                if (code == AmazonRefreshCode.SecurityGroups || code == AmazonRefreshCode.All) {
                    await EC2Utility.GetEC2SecurityGroups(EC2SecurityGroups, SVpc)
                        .ContinueWith(_ => SSecurityGroup = SettingsFirstOrDefault("", EC2SecurityGroups));
                }
                if (code == AmazonRefreshCode.KeyPairs || code == AmazonRefreshCode.All) {
                    await EC2Utility.GetEC2KeyPairs(EC2KeyPairs)
                        .ContinueWith(_ => SKey = SettingsFirstOrDefault("not implemented" /*SKey.DefKeyName*/, EC2KeyPairs));
                }
                if (code == AmazonRefreshCode.Images || code == AmazonRefreshCode.All) {
                    // TODO: Pull these from the api
                    //await EC2Utility.GetEC2Images(EC2Images);
                    EC2Images.Add(new EC2Image { Name = "TMC CDIA Development", ImageId = "ami-4212963d" });
                    EC2Images.Add(new EC2Image { Name = "TMC CDIA Integration", ImageId = "ami-c0e725bd" });
                    EC2Images.Add(new EC2Image { Name = "TMC CDIA Master", ImageId = "ami-69dc1e14" });
                    EC2Images.Add(new EC2Image { Name = "LM3 CDIA Integration", ImageId = "ami-03542d7c" });
                    SImage = SettingsFirstOrDefault(SettingsView.DefAmi, EC2Images);
                }
            } catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }

        public void RefreshEC2Instances() => RefreshAmazonData(AmazonRefreshCode.Instances);
        public void RefreshEC2AvailibilityZones() => RefreshAmazonData(AmazonRefreshCode.Zones);
        public void RefreshEC2InstanceTypes() => RefreshAmazonData(AmazonRefreshCode.Types);
        public void RefreshEC2SecurityGroups() => RefreshAmazonData(AmazonRefreshCode.SecurityGroups);
        public void RefreshEC2Vpcs() => RefreshAmazonData(AmazonRefreshCode.Vpcs);
        public void RefreshEC2Images() => RefreshAmazonData(AmazonRefreshCode.Images);
        public void RefreshEC2Subnets() => RefreshAmazonData(AmazonRefreshCode.Subnets);
        public void RefreshKeyPairInfo() => RefreshAmazonData(AmazonRefreshCode.KeyPairs);
        public void RefreshEC2AllData() => RefreshAmazonData(AmazonRefreshCode.All);

        // EC2 Command methods
        private enum AmazonCommand {
            Reboot,
            Start,
            Stop,
            Terminate,
            Launch
        }

        private async void RunAmazonCommand(AmazonCommand code) {
            try {
                _refreshTimer.Stop();
                _refreshTimer.IsEnabled = false;

                if (SInstance == null && code != AmazonCommand.Launch) 
                    return;

                switch (code) {
                    case AmazonCommand.Reboot:
                        await EC2Utility.RebootEC2Instance(SInstance.InstanceId);
                        break;
                    case AmazonCommand.Start:
                        await EC2Utility.StartEC2Instance(SInstance.InstanceId);
                        break;
                    case AmazonCommand.Stop:
                        await EC2Utility.StopEC2Instance(SInstance.InstanceId);
                        break;
                    case AmazonCommand.Terminate:
                        await EC2Utility.TerminateEC2Instance(SInstance.InstanceId);
                        break;
                    case AmazonCommand.Launch:
                        await EC2Utility.LaunchEC2Instance(SAvailabilityZone, SInstanceType,
                            SSubnet.SubnetId, SImage.ImageId, UsePublicIp, ActiveSecurityGroups,
                            StartNumber, SVpc, SKey, EC2Tags);
                        break;
                    default:
                        break;
                }

                _refreshTimer.Start();
                _refreshTimer.IsEnabled = true;
            } catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }

        public void LaunchEC2Instance() => RunAmazonCommand(AmazonCommand.Launch);
        public void TerminateEC2Instance() => RunAmazonCommand(AmazonCommand.Terminate);
        public void StartEC2Instance() => RunAmazonCommand(AmazonCommand.Start);
        public void StopEC2Instance() => RunAmazonCommand(AmazonCommand.Stop);
        public void RebootEC2Instance() => RunAmazonCommand(AmazonCommand.Reboot);

        // Private helpers
        private void ResetSelectedInstance(Instance si) {
            if (si == null) return;

            foreach (Instance instance in EC2Instances) {
                if (instance.InstanceId == si.InstanceId)
                    SInstance = instance;
            }
        }

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

    public class StringToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture) {
            string s = "";
            PlatformValues pv;

            if (value == null)
                return false;

            if (value.GetType() == typeof(string)) {
                s = (value as string);
            } else if (value.GetType() == typeof(PlatformValues)) {
                pv = (value as PlatformValues);
                s = pv.ToString();
            } else {
                throw new InvalidOperationException();
            }

            //Debug.WriteLine($"Converting {s} to boolean: result == {s.Length == 0}");
            return s.Length != 0;

        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture) {
            // You can't go back...
            return false;
        }
    }

    // Another hack to work around a bug...fook me in the goat arse!
    public class NegativeStringToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture) {
            string s = "";
            PlatformValues pv;

            if (value == null)
                return true;

            if (value.GetType() == typeof(string)) {
                s = (value as string);
            } else if (value.GetType() == typeof(PlatformValues)) {
                pv = (value as PlatformValues);
                s = pv.ToString();
            } else {
                throw new InvalidOperationException();
            }

            //Debug.WriteLine($"Converting {s} to boolean: result == {s.Length == 0}");
            return s.Length == 0;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture) {
            // You can't go back...
            return true;
        }
    }
}
