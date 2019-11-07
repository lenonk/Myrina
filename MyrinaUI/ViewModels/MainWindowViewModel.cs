using System.Collections.ObjectModel;
using MyrinaUI.Models;
using ReactiveUI;
using System.Threading.Tasks;
using MsgBox;
using Avalonia.Controls;

namespace MyrinaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<EC2InstanceModel> EC2Instances { get; }
        private EC2InstanceModel _sInstance;
        public EC2InstanceModel SInstance {
            get { return _sInstance; }
            set { this.RaiseAndSetIfChanged(ref _sInstance, value); }
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

        public MainWindowViewModel() {
            EC2Instances = new ObservableCollection<EC2InstanceModel>();
            EC2AvailabilityZones = new ObservableCollection<string>();
            EC2InstanceTypes = new ObservableCollection<string>();
            EC2Amis = new ObservableCollection<EC2AmiModel>();
            EC2Subnets = new ObservableCollection<EC2SubnetModel>();

            RefreshEC2AllData().ContinueWith(_ => InitializeComboBoxes());
        }

        public void InitializeComboBoxes() {
            SAvailabilityZone = EC2AvailabilityZones[0];
            SInstanceType = EC2InstanceTypes[0];
            SAmi = EC2Amis[0];
            SSubnet = EC2Subnets[0];

            // Hack around DataGrid's SelectionChanged event being inaccesible from Avalonia XAML
            DataGrid _grid = Avalonia.Application.Current.MainWindow.FindControl<DataGrid>("instanceGrid");
            if (_grid != null) {
                // Just in case this ever gets called twice...
                _grid.SelectionChanged -= OnRowClicked;
                _grid.SelectionChanged += OnRowClicked;
            } 
            else {
                MessageBox.Show("Unable to find grid control!");
            }
        }

        public async Task RefreshEC2Instances() {
            await EC2UtilityModel.GetEC2Instances(EC2Instances);
        }

        public async Task RefreshEC2AvailibilityZones() {
            await EC2UtilityModel.GetEC2AvailabilityZones(EC2AvailabilityZones);
        }

        public async Task RefreshEC2InstanceTypes() {
            await EC2UtilityModel.GetEC2InstanceTypes(EC2InstanceTypes);
        }

        public async Task RefreshEC2Subnets() {
            await EC2UtilityModel.GetEC2Subnets(EC2Subnets);
        }

        public async Task RefreshEC2AllData() {
            await EC2UtilityModel.GetEC2Instances(EC2Instances);
            await EC2UtilityModel.GetEC2AvailabilityZones(EC2AvailabilityZones);
            await EC2UtilityModel.GetEC2InstanceTypes(EC2InstanceTypes);
            await EC2UtilityModel.GetEC2Subnets(EC2Subnets);

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
            await EC2UtilityModel.LaunchEC2Instance(SAvailabilityZone, SInstanceType, SSubnet.SubnetId, SAmi.Value);
        }

        public void OnRowClicked(object sender, SelectionChangedEventArgs e) {
            SInstance = ((sender as DataGrid).SelectedItem as EC2InstanceModel);
            e.Handled = true;
        }
    }
}
