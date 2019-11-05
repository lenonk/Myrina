using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MyrinaUI.Models;

namespace MyrinaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<EC2InstanceModel> EC2Instances { get; }
        public ObservableCollection<string> EC2InstanceTypes { get; set;  }
        public ObservableCollection<string> EC2AvailabilityZones { get; set; }

        public string SelectedAvailabilityZone { get; set; }
        public string SelectedInstanceType { get; set; }

        public MainWindowViewModel() {
            EC2Instances = new ObservableCollection<EC2InstanceModel>();
            EC2AvailabilityZones = new ObservableCollection<string>();
            EC2InstanceTypes = new ObservableCollection<string>();
            EC2UtilityModel.GetEC2Instances(EC2Instances);
            EC2UtilityModel.GetEC2AvailabilityZones(EC2AvailabilityZones);
            EC2UtilityModel.GetEC2InstanceTypes(EC2InstanceTypes);
        }

        public void RefreshEC2Instances() {
            EC2UtilityModel.GetEC2Instances(EC2Instances);
        }
        public void LaunchEC2Instance() {
            throw new NotImplementedException();
        }
    }
}
