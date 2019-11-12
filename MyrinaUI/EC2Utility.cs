using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using MyrinaUI.ViewModels;
using System.Diagnostics;
using System.Linq;

namespace MyrinaUI.Models {
    static public class EC2Utility {
        static string AccessKey = SettingsViewModel.Settings.AccessKey;
        static string SecretKey = SettingsViewModel.Settings.SecretKey;

        static public async Task LaunchEC2Instance(string SAvailabilityZone, string SInstanceType, 
            string SSubnet, string SAmi, bool UsePublicIp, 
            ObservableCollection<SecurityGroup> sgroups, int startnum, Vpc vpc, KeyPairInfo key, ObservableCollection<Tag> tags = null) {

            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            RunInstancesRequest req = new RunInstancesRequest();
            req.Placement = new Placement();
            req.Placement.AvailabilityZone = SAvailabilityZone;
            req.InstanceType = SInstanceType;
            req.ImageId = SAmi;
            req.MinCount = 1;
            req.MaxCount = startnum;
            req.SecurityGroupIds = new List<string>();
            if (key.KeyFingerprint != "0xdeadbeef")
                req.KeyName = key.KeyName;

            if (SSubnet == "(Default)") SSubnet = "";
            if (UsePublicIp) {
                req.NetworkInterfaces = new List<InstanceNetworkInterfaceSpecification>();
                req.NetworkInterfaces.Add(new InstanceNetworkInterfaceSpecification() {
                    AssociatePublicIpAddress = true,
                    DeleteOnTermination = true,
                    DeviceIndex = 0,
                    SubnetId = SSubnet
                }); 
                foreach (var sgroup in sgroups) {
                    req.NetworkInterfaces[0].Groups.Add(sgroup.GroupId);
                }
            }
            else {
                req.SubnetId = SSubnet;
                foreach (var sgroup in sgroups) {
                    req.SecurityGroupIds.Add(sgroup.GroupId);
                }
            }

            req.TagSpecifications = new List<TagSpecification>();
            req.TagSpecifications.Add(new TagSpecification() {
                ResourceType = ResourceType.Instance,
                Tags = new List<Tag>()
            });

            // Add Default Tags
            foreach (Tag t in SettingsViewModel.Settings.DefTags) {
                req.TagSpecifications[0].Tags.Add(t);
            }

            // Add instance specific tags
            if (tags != null) {
                foreach (Tag newtag in tags) {
                    bool found = false;
                    foreach (Tag deftag in req.TagSpecifications[0].Tags) {
                        if (deftag.Key == newtag.Key) {
                            req.TagSpecifications[0].Tags.Remove(deftag);
                            found = true;
                            break;
                        }
                        if (found) break;
                    }

                    req.TagSpecifications[0].Tags.Add(newtag);
                }
            }

            try {
                RunInstancesResponse resp = await client.RunInstancesAsync(req);
                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    foreach (Instance instance in resp.Reservation.Instances) {
                        MessageBox.Show($"Sucessfully started instance id: {instance.InstanceId}", MainViewModel.MainWindow);
                    }

                    return;
                }

                MessageBox.Show($"EC2 function: RunInstancesAsync() " +
                    $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }
        
        static public async Task TerminateEC2Instance(string instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            TerminateInstancesRequest req = new TerminateInstancesRequest();
            req.InstanceIds.Add(instance);
            try {
                TerminateInstancesResponse resp = await client.TerminateInstancesAsync(req);
                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    MessageBox.Show($"Sucessfully requested termination of instance id: {instance}", MainViewModel.MainWindow);
                    return;
                }

                MessageBox.Show($"EC2 function: TerminateInstancesAsync() " +
                    $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }
        static public async Task StartEC2Instance(string instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            StartInstancesRequest req = new StartInstancesRequest();
            req.InstanceIds.Add(instance);
            try {
                StartInstancesResponse resp = await client.StartInstancesAsync(req);
                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    MessageBox.Show($"Sucessfully requested start of instance id: {instance}", MainViewModel.MainWindow);
                    return;
                }

                MessageBox.Show($"EC2 function: StartInstancesAsync() " +
                    $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }

        static public async Task StopEC2Instance(string instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            StopInstancesRequest req = new StopInstancesRequest();
            req.InstanceIds.Add(instance);
            try {
                StopInstancesResponse resp = await client.StopInstancesAsync(req);
                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    MessageBox.Show($"Sucessfully requested stop of instance id: {instance}", MainViewModel.MainWindow);
                    return;
                }

                MessageBox.Show($"EC2 function: StopInstancesAsync() " +
                    $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }
        static public async Task RebootEC2Instance(string instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            RebootInstancesRequest req = new RebootInstancesRequest();
            req.InstanceIds.Add(instance);
            try {
                RebootInstancesResponse resp = await client.RebootInstancesAsync(req);
                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    MessageBox.Show($"Sucessfully requested reboot of instance id: {instance}", MainViewModel.MainWindow);
                    return;
                }

                MessageBox.Show($"EC2 function: RebootInstancesAsync() " +
                    $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }

        static public async Task<int> GetEC2InstanceTypes(ObservableCollection<string> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var _types = new List<string>();

            var req = new DescribeSpotPriceHistoryRequest();
            DescribeSpotPriceHistoryResponse resp;

            var result = await Task.Run(async () => {
                try { resp = await client.DescribeSpotPriceHistoryAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeSpotPriceHistoryAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                foreach (SpotPrice price in resp.SpotPriceHistory) {
                    if (!_types.Contains(price.InstanceType))
                        _types.Add(price.InstanceType);
                }

                return 0;
            });

            col.Clear();
            _types.Sort();
            _types.ForEach(x => col.Add(x));

            return result;
        }

        static public async Task<int> GetEC2InstanceTags(Instance instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var _tags = new List<Tag>();

            var req = new DescribeTagsRequest();
            DescribeTagsResponse resp;

            var result = await Task.Run(async () => {
                req.Filters.Add(new Filter() {
                    Name = "resource-id",
                    Values = new List<string> { instance.InstanceId }
                });

                try { resp = await client.DescribeTagsAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeTagsAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                resp.Tags.ForEach(x => _tags.Add(new Tag(x.Key, x.Value)));
                return 0;
            });

            _tags.ForEach(x => instance.Tags.Add(x));
            return result;
        }
        
        static public async Task<int> GetEC2Instances(ObservableCollection<Instance> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var _instances = new List<Instance>();

            DescribeInstancesRequest req = new DescribeInstancesRequest();
            DescribeInstancesResponse resp;

            var result = await Task.Run(async () => {
                try { resp = await client.DescribeInstancesAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeInstanceAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                foreach (Reservation res in resp.Reservations) {
                    foreach (Instance instance in res.Instances) {
                        SortTagsAndAddName(instance.Tags, "");
                        _instances.Add(instance);
                    }
                }
                
                return 0;
            });

            col.Clear();
            _instances.ForEach(x => col.Add(x));

            return result;
        }

        static public async Task<int> GetEC2SecurityGroups(ObservableCollection<SecurityGroup> col, Vpc vpc = null) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var _groups = new List<SecurityGroup>();
            
            var req = new DescribeSecurityGroupsRequest();
            DescribeSecurityGroupsResponse resp;

            var result = await Task.Run(async () => {
                try { resp = await client.DescribeSecurityGroupsAsync(); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeSecurityGroupsAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                resp.SecurityGroups.Sort((a, b) => string.Compare(a.GroupName, b.GroupName));
                resp.SecurityGroups.ForEach((x) => {
                if (vpc == null || x.VpcId == vpc.VpcId)
                    _groups.Add(x);
                });

                return 0;
            });

            col.Clear();
            _groups.ForEach(x => col.Add(x));

            return result;
        }

        static public async Task<int> GetEC2Vpcs(ObservableCollection<Vpc> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var _vpcs = new List<Vpc>();
            var req = new DescribeVpcsRequest();
            DescribeVpcsResponse resp;

            var result = await Task.Run(async () => {
                try { resp = await client.DescribeVpcsAsync(); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeSecurityGroupsAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                resp.Vpcs.ForEach((x) => {
                    string value;
                    if (x.IsDefault) {
                        value = "(Default)";
                        _vpcs.Insert(0, x);
                    } 
                    else {
                        value = x.VpcId;
                        _vpcs.Add(x);
                    }

                    SortTagsAndAddName(x.Tags, value);
                });

                return 0;
            });

            col.Clear();
            _vpcs.ForEach(x => col.Add(x));

            return result;
        }

        static public async Task<int> GetEC2AvailabilityZones(ObservableCollection<string> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var _zones = new List<string>();

            var req = new DescribeAvailabilityZonesRequest();
            DescribeAvailabilityZonesResponse resp;

            var result = await Task.Run(async () => { 
                try { resp = await client.DescribeAvailabilityZonesAsync(req); }
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeAvailabilityZonesAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                foreach (AvailabilityZone zone in resp.AvailabilityZones) {
                    if (!_zones.Contains(zone.ZoneName) && zone.State.ToString() == "available")
                        _zones.Add(zone.ZoneName);
                }

                return 0;
            });

            col.Clear();
            _zones.Sort();
            _zones.ForEach(x => col.Add(x));

            return result;
        }

        static public async Task<int> GetEC2Subnets(ObservableCollection<Subnet> col, Vpc vpc = null) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var _subnets = new List<Subnet>();

            var req = new DescribeSubnetsRequest();
            DescribeSubnetsResponse resp;

            var result = await Task.Run(async () => {
                try { resp = await client.DescribeSubnetsAsync(); } 
                catch (AmazonEC2Exception e) { throw e; }

                // It's worse than that. He's dead, Jim!
                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeSubnetsAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                resp.Subnets.ForEach((x) => {
                    string value = x.AvailabilityZone;
                    if (x.DefaultForAz)
                        value = $"(Default for {x.AvailabilityZone}";

                    SortTagsAndAddName(x.Tags, value);

                    if (vpc == null || x.VpcId == vpc.VpcId)
                        _subnets.Add(x);
                });

                return 0;
            });

            col.Clear();
            if (vpc != null && vpc.IsDefault) {
                var snet = new Subnet();
                SortTagsAndAddName(snet.Tags, "No Preference (default in any zone)");
                col.Add(snet);
            }
            _subnets.ForEach(x => col.Add(x));

            return result;
        }

        static public async Task<int> GetEC2KeyPairs(ObservableCollection<KeyPairInfo> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var _pairs = new List<KeyPairInfo>();

            var req = new DescribeKeyPairsRequest();
            DescribeKeyPairsResponse resp;

            var result = await Task.Run(async () => {
                try { resp = await client.DescribeKeyPairsAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeKeyPairsAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                resp.KeyPairs.ForEach((x) => _pairs.Add(x));

                return 0;
            });

            col.Clear();
            _pairs.Sort((a, b) => string.Compare(a.KeyName, b.KeyName));
            col.Add(new KeyPairInfo() { KeyName = "(None Selected)", KeyFingerprint = "0xdeadbeef" });
            _pairs.ForEach(x => col.Add(x));

            return result;
        }

        static private void SortTagsAndAddName(List<Tag> tags, string value = null) {
            if (tags == null)
                tags = new List<Tag>();

            if (value != null && value != string.Empty)
                if (!tags.Exists(x => string.Equals(x.Key, "Name", StringComparison.OrdinalIgnoreCase)))
                    tags.Add(new Tag() { Key = "Name", Value = value });

            tags.Sort((a, b) => {
                if (string.Equals(a.Key, "name", StringComparison.OrdinalIgnoreCase))
                    return -1;
                if (string.Equals(b.Key, "name", StringComparison.OrdinalIgnoreCase))
                    return 1;

                return string.Compare(a.Key, b.Key);
            });
        }

        /*static private void AddFilter(List<Filter> flist, string name, string value) {
            Filter f = new Filter();
            f.Values = new List<string>();

            f.Name = name;
            f.Values.Add(value);

            flist.Add(f);
        }

        static public async Task GetEC2Amis(ObservableCollection<Image> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            DescribeImagesRequest req = new DescribeImagesRequest();
            req.ExecutableUsers = new List<string>();
            req.ExecutableUsers.Add("self");

            req.Owners = new List<string>();
            req.Owners.Add("self");

            req.Filters = new List<Filter>();
            AddFilter(req.Filters, "state", "available");
            AddFilter(req.Filters, "is-public", "false");
            AddFilter(req.Filters, "owner-alias", "amazon");
            AddFilter(req.Filters, "image-type", "machine");
            AddFilter(req.Filters, "architecture", "x86_64");

            col.Clear();
            try {
                DescribeImagesResponse resp = await client.DescribeImagesAsync();

                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    resp.Images.ForEach((x) => col.Add(x));
                    return;
                }

                MessageBox.Show($"EC2 function: DescribeSubnetsAsync() " +
                    $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }*/
    }
}
