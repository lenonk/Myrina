using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MyrinaUI.Models;
using ReactiveUI;
using System.Threading.Tasks;

namespace MyrinaUI.ViewModels
{
    public class EC2Itype {
        public string Id;
        public string Zone;
    }

    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<EC2InstanceModel> EC2Instances { get; }
        public ObservableCollection<string> EC2InstanceTypes { get; }
        public ObservableCollection<string> EC2AvailabilityZones { get; }
        public ObservableCollection<EC2AmiModel> EC2Amis { get; }

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

        private EC2AmiModel _sAmi;
        public EC2AmiModel SAmi {
            get { return _sAmi; }
            set { this.RaiseAndSetIfChanged(ref _sAmi, value); }
        }        

        public MainWindowViewModel() {
            EC2Instances = new ObservableCollection<EC2InstanceModel>();
            EC2AvailabilityZones = new ObservableCollection<string>();
            EC2InstanceTypes = new ObservableCollection<string>();
            EC2Amis = new ObservableCollection<EC2AmiModel>();

            RefreshEC2AllData().ContinueWith(_ => InitializeComboBoxes());
        }

        public void InitializeComboBoxes() {
            SInstanceType = EC2InstanceTypes[0];
            SAvailabilityZone = EC2AvailabilityZones[0];
            SAmi = EC2Amis[0];
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
        public async Task RefreshEC2AllData() {
            await EC2UtilityModel.GetEC2Instances(EC2Instances);
            await EC2UtilityModel.GetEC2AvailabilityZones(EC2AvailabilityZones);
            await EC2UtilityModel.GetEC2InstanceTypes(EC2InstanceTypes);

            // Hardcode AMIs for now, but will pull list from AWS later if necessary
            GetEC2Amis();
        }

        public void GetEC2Amis() {
            EC2Amis.Add(new EC2AmiModel { Name = "TMC CDIA Development", Value = "ami-4212963d" });    
            EC2Amis.Add(new EC2AmiModel { Name = "TMC CDIA Integration", Value = "ami-c0e725bd" });    
            EC2Amis.Add(new EC2AmiModel { Name = "TMC CDIA Master",      Value = "ami-69dc1e14" });    
            EC2Amis.Add(new EC2AmiModel { Name = "LM3 CDIA Integration", Value = "ami-03542d7c" });    
        }

        public void LaunchEC2Instance() {
            throw new NotImplementedException();
        }
    }
}
