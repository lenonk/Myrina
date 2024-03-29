﻿using Amazon.EC2;
using Amazon.EC2.Model;
using MyrinaUI.Services;
using MyrinaUI.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reflection;

namespace MyrinaUI.ViewModels {
    public class LaunchParamsViewModel : ViewModelBase {
        // Public class members
        public ObservableCollection<string> EC2InstanceTypes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> EC2AvailabilityZones { get; } = new ObservableCollection<string>();
        public ObservableCollection<Image> EC2Images { get; } = new ObservableCollection<Image>();
        public ObservableCollection<Subnet> EC2Subnets { get; private set; } = new ObservableCollection<Subnet>();
        public ObservableCollection<SecurityGroup> EC2SecurityGroups { get; set; } = new ObservableCollection<SecurityGroup>();
        public ObservableCollection<Vpc> EC2Vpcs { get; set; } = new ObservableCollection<Vpc>();
        public ObservableCollection<KeyPairInfo> EC2KeyPairs { get; set; } = new ObservableCollection<KeyPairInfo>();
        public ObservableCollection<Tag> EC2Tags { get; } = new ObservableCollection<Tag>();
        public ObservableCollection<SecurityGroup> ActiveSecurityGroups { get; } = new ObservableCollection<SecurityGroup>();
        
        // Properties
        #region Properties
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

        private Image _sImage;
        public Image SImage {
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
        public LaunchParamsViewModel() {
            EventSystem.Subscribe<SettingsChanged>((x) => { RefreshEC2AllData(); });

            RefreshEC2AllData();

            this.WhenAnyValue(x => x.SAvailabilityZone)
                .Where(x => x != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => {
                    RefreshEC2Images();
                    RefreshEC2InstanceTypes();
                    RefreshEC2Vpcs();
                    RefreshEC2KeyPairInfo();

                    EventSystem.Publish(new RefreshInstanceList() { value = x });
                }
            );

            this.WhenAnyValue(x => x.SVpc)
                .Where(x => x != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe((x) => {
                    ActiveSecurityGroups.Clear();
                    RefreshEC2Subnets(); 
                    RefreshEC2SecurityGroups(); 
                }
            );
        }

        // Methods
        public void AddSecurityGroup(SecurityGroup group) => ActiveSecurityGroups.Add(group);
        public void DeleteSecurityGroup(SecurityGroup group) => ActiveSecurityGroups.Remove(group);
        public void AddTag() => EC2Tags.Add(new Tag() { Key = "", Value = "" });
        public void DeleteTag(Tag key) => EC2Tags.Remove(key);
        public void ShowSettings() => ViewFinder.Get<SettingsView>().Show();

        public async void LaunchEC2Instance() {
            try {
                var msg = await EC2Service.Instance.LaunchEC2Instance(SAvailabilityZone, SInstanceType,
                    SSubnet.SubnetId, SImage.ImageId, UsePublicIp, ActiveSecurityGroups,
                    StartNumber, SVpc, SKey, EC2Tags, SAvailabilityZone);
                    EventSystem.Publish(new RefreshInstanceList() { value = SAvailabilityZone });
                    LogViewModel.LogView.Log(msg);
            } 
            catch (Exception e) when 
                (e is AmazonEC2Exception ||
                 e is System.Net.Http.HttpRequestException ||
                 e is System.NullReferenceException) {
                LogViewModel.LogView.Log(e.Message);
            }
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
                    await EC2Service.Instance.GetEC2Vpcs(EC2Vpcs, SAvailabilityZone)
                        .ContinueWith(_ => SVpc = SettingsFirstOrDefault(Settings.Current.Vpc, EC2Vpcs, "VpcId"));
                }
                if (code == AmazonRefreshCode.Zones || code == AmazonRefreshCode.All) {
                    await EC2Service.Instance.GetEC2AvailabilityZones(EC2AvailabilityZones)
                        .ContinueWith(_ => SAvailabilityZone = SettingsFirstOrDefault(Settings.Current.Zone, EC2AvailabilityZones));
                }
                if (code == AmazonRefreshCode.Types || code == AmazonRefreshCode.All) {
                    await EC2Service.Instance.GetEC2InstanceTypes(EC2InstanceTypes)
                        .ContinueWith(_ => SInstanceType = SettingsFirstOrDefault(Settings.Current.InstanceType, EC2InstanceTypes));
                }
                if (code == AmazonRefreshCode.Subnets || code == AmazonRefreshCode.All) {
                    await EC2Service.Instance.GetEC2Subnets(EC2Subnets, SVpc, SAvailabilityZone)
                        .ContinueWith(_ => SSubnet = SettingsFirstOrDefault("", EC2Subnets));
                }
                if (code == AmazonRefreshCode.SecurityGroups || code == AmazonRefreshCode.All) {
                    await EC2Service.Instance.GetEC2SecurityGroups(EC2SecurityGroups, SVpc, SAvailabilityZone)
                        .ContinueWith(_ => SSecurityGroup = SettingsFirstOrDefault("", EC2SecurityGroups));
                }
                if (code == AmazonRefreshCode.KeyPairs || code == AmazonRefreshCode.All) {
                    await EC2Service.Instance.GetEC2KeyPairs(EC2KeyPairs, SAvailabilityZone)
                        .ContinueWith(_ => SKey = SettingsFirstOrDefault(Settings.Current.KeyPair, EC2KeyPairs, "KeyName"));
                }
                if (code == AmazonRefreshCode.Images || code == AmazonRefreshCode.All) {
                    // TODO: Pull these from the api
                    await EC2Service.Instance.GetEC2Images(EC2Images, SAvailabilityZone);
                    SImage = SettingsFirstOrDefault(Settings.Current.Image, EC2Images, "ImageId");
                }
            } 
            catch (Exception e) when 
                (e is AmazonEC2Exception ||
                 e is System.Net.Http.HttpRequestException) {
                LogViewModel.LogView.Log(e.Message);
            }
        }

        public void RefreshEC2AvailibilityZones() => RefreshAmazonData(AmazonRefreshCode.Zones);
        public void RefreshEC2InstanceTypes() => RefreshAmazonData(AmazonRefreshCode.Types);
        public void RefreshEC2SecurityGroups() => RefreshAmazonData(AmazonRefreshCode.SecurityGroups);
        public void RefreshEC2Vpcs() => RefreshAmazonData(AmazonRefreshCode.Vpcs);
        public void RefreshEC2Images() => RefreshAmazonData(AmazonRefreshCode.Images);
        public void RefreshEC2Subnets() => RefreshAmazonData(AmazonRefreshCode.Subnets);
        public void RefreshEC2KeyPairInfo() => RefreshAmazonData(AmazonRefreshCode.KeyPairs);
        public void RefreshEC2AllData() => RefreshAmazonData(AmazonRefreshCode.All);

        // Private helpers
        private T SettingsFirstOrDefault<T>(string value, ObservableCollection<T> col, string property = null) {
            if (!string.IsNullOrEmpty(value)) {
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

            return default;
        }
    }
}
