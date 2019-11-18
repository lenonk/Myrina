using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace MyrinaUI.Services {
    public sealed class EC2Service {
        private static readonly Lazy<EC2Service> lazy = 
            new Lazy<EC2Service>(() => new EC2Service());

        public static EC2Service Instance { get { return lazy.Value; } }

        private EC2Service() {

            EventSystem.Subscribe<AccessKeyChanged>((x) => { AccessKey = x.value; });
            EventSystem.Subscribe<SecretKeyChanged>((x) => { SecretKey = x.value; });
        }

        private string AccessKey = Settings.Current.AccessKey;
        private string SecretKey = Settings.Current.SecretKey;

        public async Task<string> LaunchEC2Instance(string SAvailabilityZone, string SInstanceType, 
            string SSubnet, string SAmi, bool UsePublicIp, 
            ObservableCollection<SecurityGroup> sgroups, int startnum, Vpc vpc, 
            KeyPairInfo key, ObservableCollection<Tag> tags = null) {

            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            RunInstancesRequest req = new RunInstancesRequest();
            req.Placement = new Placement();
            req.Placement.AvailabilityZone = SAvailabilityZone;
            req.InstanceType = SInstanceType;
            req.ImageId = SAmi;
            req.MinCount = 1;
            req.MaxCount = startnum;
            if (key.KeyFingerprint != "0xdeadbeef")
                req.KeyName = key.KeyName;

            SetSubnetAndSecurityGroups(req, SSubnet, sgroups, UsePublicIp);

            // Add default tags
            AddTagsToInstance(req, Settings.Current.Tags);
            // Add instance specific tags, overwriting defaults
            AddTagsToInstance(req, tags);

            RunInstancesResponse resp;

            var result = await Task.Run(async () => {
                try { resp = await client.RunInstancesAsync(req); }
                catch (AmazonEC2Exception e) { throw e; }
                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: RunInstancesAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                string msgs = "";
                foreach (Instance instance in resp.Reservation.Instances) {
                    msgs += $"Sucessfully started instance id: {instance.InstanceId}";
                }

                return msgs;
            });

            return result;
        }
        
        public async Task<string> TerminateEC2Instance(string instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var req = new TerminateInstancesRequest();

            TerminateInstancesResponse resp;
            req.InstanceIds.Add(instance);

            var result = await Task.Run(async () => {
                try { resp = await client.TerminateInstancesAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: TerminateInstancesAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }
                return $"Sucessfully requested termination of instance id: {instance}";
            });

            return result;
        }

        public async Task<string> StartEC2Instance(string instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var req = new StartInstancesRequest();

            StartInstancesResponse resp;
            req.InstanceIds.Add(instance);

            var result = await Task.Run(async () => {
                try { resp = await client.StartInstancesAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: StartInstancesAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                return $"Sucessfully requested start of instance id: {instance}";
            });

            return result;
        }

        public async Task<string> StopEC2Instance(string instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var req = new StopInstancesRequest();

            StopInstancesResponse resp;
            req.InstanceIds.Add(instance);

            var result = await Task.Run(async () => {
                try { resp = await client.StopInstancesAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: StopInstancesAsync() " + 
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                return $"Sucessfully requested stop of instance id: {instance}";
            });

            return result;
        }

        public async Task<string> RebootEC2Instance(string instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var req = new RebootInstancesRequest();

            req.InstanceIds.Add(instance);
            RebootInstancesResponse resp;

            var result = await Task.Run(async () => {
                try { resp = await client.RebootInstancesAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: RebootInstancesAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                return $"Sucessfully requested reboot of instance id: {instance}";
            });

            return result;
        }

        public async Task<int> GetEC2InstanceTypes(ObservableCollection<string> col) {
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

        public async Task<int> GetEC2InstanceTags(Instance instance) {
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
        
        public async Task<int> GetEC2Instances(ObservableCollection<Instance> col) {
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

        public async Task<int> GetEC2SecurityGroups(ObservableCollection<SecurityGroup> col, Vpc vpc = null) {
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

        public async Task<int> GetEC2Vpcs(ObservableCollection<Vpc> col) {
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

        public async Task<int> GetEC2AvailabilityZones(ObservableCollection<string> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            var _zones = new List<string>();

            var req = new DescribeAvailabilityZonesRequest();
            DescribeAvailabilityZonesResponse resp;

            var result = await Task.Run(async () => { 
                try { resp = await client.DescribeAvailabilityZonesAsync(req); }
                catch (AmazonEC2Exception e) {
                    throw e;
                }

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

        public async Task<int> GetEC2Subnets(ObservableCollection<Subnet> col, Vpc vpc = null) {
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

        public async Task<int> GetEC2KeyPairs(ObservableCollection<KeyPairInfo> col) {
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

        #region Helper Functions
        private void SortTagsAndAddName(List<Tag> tags, string value = null) {
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

        private void AddTagsToInstance(RunInstancesRequest req, 
            ObservableCollection<Tag> tags) {
            if (tags == null) return;

            if (req.TagSpecifications.Count == 0) {
                req.TagSpecifications.Add(new TagSpecification() {
                    ResourceType = ResourceType.Instance,
                    Tags = new List<Tag>()
                });
            }
            var taglist = req.TagSpecifications[0].Tags;

            tags.ToList().ForEach((tag) => {
                taglist.Remove(taglist.Where((x) => x.Key == tag.Key).FirstOrDefault());
                req.TagSpecifications[0].Tags.Add(tag);
            });

        }

        private void SetSubnetAndSecurityGroups(RunInstancesRequest req, string subnet, 
            ObservableCollection<SecurityGroup> groups, bool publicIp) {
            if (groups.Count > 0)
                req.SecurityGroupIds = new List<string>();
            
            if (publicIp) {
                req.NetworkInterfaces = new List<InstanceNetworkInterfaceSpecification>();
                req.NetworkInterfaces.Add(new InstanceNetworkInterfaceSpecification() {
                    AssociatePublicIpAddress = true,
                    DeleteOnTermination = true,
                    DeviceIndex = 0,
                    SubnetId = subnet
                }); 
                foreach (var sgroup in groups) {
                    req.NetworkInterfaces[0].Groups.Add(sgroup.GroupId);
                }
            }
            else {
                req.SubnetId = subnet;
                foreach (var sgroup in groups) {
                    req.SecurityGroupIds.Add(sgroup.GroupId);
                }
            }
        }
        #endregion

        /* private void AddFilter(List<Filter> flist, string name, string value) {
            Filter f = new Filter();
            f.Values = new List<string>();

            f.Name = name;
            f.Values.Add(value);

            flist.Add(f);
        }

         public async Task GetEC2Amis(ObservableCollection<Image> col) {
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
