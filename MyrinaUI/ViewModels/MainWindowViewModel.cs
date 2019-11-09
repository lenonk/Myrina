using System.Collections.ObjectModel;
using MyrinaUI.Models;
using ReactiveUI;
using System.Threading.Tasks;
using MsgBox;
using Avalonia.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using Avalonia.Data.Converters;
using System;
using System.Diagnostics;
using Amazon.EC2;
using Avalonia.Threading;

namespace MyrinaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase {
        public static Window MainWindow;
        private DispatcherTimer _refreshTimer = new DispatcherTimer();

        public ObservableCollection<EC2InstanceModel> EC2Instances { get; }
        private EC2InstanceModel _sInstance;
        public EC2InstanceModel SInstance {
            get { return _sInstance; }
            set {
                _sInstance = value;
                this.RaisePropertyChanged(); 
            }
        }

        public ObservableCollection<string> EC2InstanceTypes { get; }
        private string _sInstanceType;
        public string SInstanceType {
            get { return _sInstanceType; }
            set { this.RaiseAndSetIfChanged(ref _sInstanceType, value); }
        }

        public ObservableCollection<string> EC2AvailabilityZones { get; }
        private string _sAvailabilityZone;
        public string SAvailabilityZone {
            get { return _sAvailabilityZone; }
            set { this.RaiseAndSetIfChanged(ref _sAvailabilityZone, value); }
        }

        public ObservableCollection<EC2AmiModel> EC2Amis { get; }
        private EC2AmiModel _sAmi;
        public EC2AmiModel SAmi {
            get { return _sAmi; }
            set { this.RaiseAndSetIfChanged(ref _sAmi, value); }
        }        

        public ObservableCollection<EC2SubnetModel> EC2Subnets { get; }
        private EC2SubnetModel _sSubnet;
        public EC2SubnetModel SSubnet {
            get { return _sSubnet; }
            set { this.RaiseAndSetIfChanged(ref _sSubnet, value); }
        }       

        public MainWindowViewModel(Window parent) {
            EC2Instances = new ObservableCollection<EC2InstanceModel>();
            EC2AvailabilityZones = new ObservableCollection<string>();
            EC2InstanceTypes = new ObservableCollection<string>();
            EC2Amis = new ObservableCollection<EC2AmiModel>();
            EC2Subnets = new ObservableCollection<EC2SubnetModel>();

            MainWindow = parent;

            RefreshEC2AllData().ContinueWith(_ => InitializeComboBoxes());

            _refreshTimer.Interval = TimeSpan.FromSeconds(30);
            _refreshTimer.Tick += async (sender, e) => { await RefreshEC2Instances(); };
            _refreshTimer.Start();
        }

        public void InitializeComboBoxes() {
            SAvailabilityZone = EC2AvailabilityZones[0];
            SInstanceType = EC2InstanceTypes[0];
            SAmi = EC2Amis[0];
            SSubnet = EC2Subnets[0];
        }

        public async Task RefreshEC2Instances() {
            try {
                Debug.WriteLine("Refreshing EC2 instances...");
                EC2InstanceModel si = SInstance;
                await EC2Utility.GetEC2Instances(EC2Instances).ContinueWith(_ => ResetSelectedInstance(si));
                Debug.WriteLine("Done refreshing EC2 instances...");
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }

        public void ResetSelectedInstance(EC2InstanceModel si) {
            if (si == null) return;

            foreach (EC2InstanceModel instance in EC2Instances) {
                if (instance.Id == si.Id)
                    SInstance = instance;
            }
        }
        public async Task RefreshEC2AvailibilityZones() {
            try {
                await EC2Utility.GetEC2AvailabilityZones(EC2AvailabilityZones);
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }

        public async Task RefreshEC2InstanceTypes() {
            try {
                await EC2Utility.GetEC2InstanceTypes(EC2InstanceTypes);
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }

        public async Task RefreshEC2Subnets() {
            try {
                await EC2Utility.GetEC2Subnets(EC2Subnets);
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }

        public async Task RefreshEC2AllData() {
            await RefreshEC2Instances();
            await RefreshEC2AvailibilityZones();
            await RefreshEC2InstanceTypes();
            await RefreshEC2Subnets();

            // Hardcode AMIs for now, but will pull list from AWS later if necessary
            GetEC2Amis();
        }

        public void GetEC2Amis() {
            EC2Amis.Add(new EC2AmiModel { Name = "TMC CDIA Development", Value = "ami-4212963d" });    
            EC2Amis.Add(new EC2AmiModel { Name = "TMC CDIA Integration", Value = "ami-c0e725bd" });    
            EC2Amis.Add(new EC2AmiModel { Name = "TMC CDIA Master",      Value = "ami-69dc1e14" });    
            EC2Amis.Add(new EC2AmiModel { Name = "LM3 CDIA Integration", Value = "ami-03542d7c" });    
        }
        public async Task LaunchEC2Instance() {
            // TODO:  Report the info for the started instance back to the user
            try {
                await EC2Utility.LaunchEC2Instance(SAvailabilityZone, SInstanceType, SSubnet.SubnetId, SAmi.Value);
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }
        public async Task TerminateEC2Instance() {
            // TODO:  Report the info for the started instance back to the user
            try {
                _refreshTimer.Stop();
                _refreshTimer.IsEnabled = false;

                if (SInstance == null) return;
                Debug.WriteLine($"Terminating EC2 Instance Id: {SInstance.Id}");
                await EC2Utility.TerminateEC2Instance(SInstance.Id);

                _refreshTimer.Start();
                _refreshTimer.IsEnabled = true;
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }
        public async Task StartEC2Instance() {
            // TODO:  Report the info for the started instance back to the user
            try {
                _refreshTimer.Stop();
                _refreshTimer.IsEnabled = false;

                if (SInstance == null) return;
                Debug.WriteLine($"Starting EC2 Instance Id: {SInstance.Id}");
                await EC2Utility.StartEC2Instance(SInstance.Id);

                _refreshTimer.Start();
                _refreshTimer.IsEnabled = true;
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }

        public async Task StopEC2Instance() {
            // TODO:  Report the info for the started instance back to the user
            try {
                _refreshTimer.Stop();
                _refreshTimer.IsEnabled = false;

                if (SInstance == null) return;
                Debug.WriteLine($"Stopping EC2 Instance Id: {SInstance.Id}");
                await EC2Utility.StopEC2Instance(SInstance.Id);

                _refreshTimer.Start();
                _refreshTimer.IsEnabled = true;
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
        }
        public async Task RebootEC2Instance() {
            // TODO:  Report the info for the started instance back to the user
            try {
                _refreshTimer.Stop();
                _refreshTimer.IsEnabled = false;

                if (SInstance == null) return;
                Debug.WriteLine($"Rebooting EC2 Instance Id: {SInstance.Id}");
                await EC2Utility.RebootEC2Instance(SInstance.Id);

                _refreshTimer.Start();
                _refreshTimer.IsEnabled = true;
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message, MainWindow);
            }
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
            } 
            else if (value.GetType() == typeof(PlatformValues)) {
                pv = (value as PlatformValues);
                s = pv.ToString();
            }
            else {
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
            } 
            else if (value.GetType() == typeof(PlatformValues)) {
                pv = (value as PlatformValues);
                s = pv.ToString();
            }
            else {
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
