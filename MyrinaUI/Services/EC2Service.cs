using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using DynamicData;
using System.Diagnostics;
using System.Reflection;

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

        public string NormalizeZoneToEndpoint(string zone) {
            if (zone == null)
                zone = "us-east-1a";

            if (!char.IsDigit(zone[zone.Length - 1])) { zone = zone.Remove(zone.Length - 1, 1); }

            return zone;
        }

        public async Task<string> LaunchEC2Instance(string SAvailabilityZone, string SInstanceType, 
            string SSubnet, string SAmi, bool UsePublicIp, 
            ObservableCollection<SecurityGroup> sgroups, int startnum, Vpc vpc, 
            KeyPairInfo key, ObservableCollection<Tag> tags = null, string zone = "us-east-1") {

            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
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
            // Add a tag to sign this as created by Myrina, and the user who did it.
            AddSignatureTag(req);

            RunInstancesResponse resp;

            var result = await Task.Run(async () => {
                try { resp = await client.RunInstancesAsync(req); }
                catch (AmazonEC2Exception e) { throw e; }
                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: RunInstancesAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                List<string> _ids = new List<string>();
                foreach (Instance instance in resp.Reservation.Instances) {
                    _ids.Add(instance.InstanceId);
                }

                return $"Sucessfully started instance id(s): {string.Join(", ", _ids)}\n";
            });

            return result;
        }
        
        public async Task<string> TerminateEC2Instance(System.Collections.IList items, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
            var req = new TerminateInstancesRequest();

            foreach (Instance i in items)
                req.InstanceIds.Add(i.InstanceId);

            TerminateInstancesResponse resp;
            var result = await Task.Run(async () => {
                try { resp = await client.TerminateInstancesAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: TerminateInstancesAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                return $"Sucessfully requested termination of instance id(s): {string.Join(", ", req.InstanceIds)}";
            });

            return result;
        }

        public async Task<string> StartEC2Instance(System.Collections.IList items, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
            var req = new StartInstancesRequest();

            foreach (Instance i in items)
                req.InstanceIds.Add(i.InstanceId);

            StartInstancesResponse resp;
            var result = await Task.Run(async () => {
                try { resp = await client.StartInstancesAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: StartInstancesAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                return $"Sucessfully requested start of instance id(s): {string.Join(", ", req.InstanceIds)}";
            });

            return result;
        }

        public async Task<string> StopEC2Instance(System.Collections.IList items, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
            var req = new StopInstancesRequest();

            foreach (Instance i in items)
                req.InstanceIds.Add(i.InstanceId);

            StopInstancesResponse resp;
            var result = await Task.Run(async () => {
                try { resp = await client.StopInstancesAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: StopInstancesAsync() " + 
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                return $"Sucessfully requested stop of instance id(s): {string.Join(", ", req.InstanceIds)}";
            });

            return result;
        }

        public async Task<string> RebootEC2Instance(System.Collections.IList items, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
            var req = new RebootInstancesRequest();

            foreach (Instance i in items)
                req.InstanceIds.Add(i.InstanceId);

            RebootInstancesResponse resp;
            var result = await Task.Run(async () => {
                try { resp = await client.RebootInstancesAsync(req); } 
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: RebootInstancesAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                return $"Sucessfully requested reboot of instance id(s): {string.Join(", ", req.InstanceIds)}";
            });

            return result;
        }

        public async Task<int> GetEC2InstanceTypes(ObservableCollection<string> col) {
            var _types = new List<string>();

            var result = await Task.Run(async () => {
                Type type = typeof(InstanceType);
                foreach (var t in type.GetFields(BindingFlags.Public | BindingFlags.Static)) {
                    _types.Add(t.GetValue(t.Name).ToString());
                }
                return 0;
            });

            col.Clear();
            _types.Sort();
            _types.ForEach(x => col.Add(x));

            return result;
        }

        public async Task<int> GetEC2InstanceTags(Instance instance, string zone = "us-east-1") {
            NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
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
        
        public async Task<int> GetEC2Instances(SourceList<Instance> col, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
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

        public async Task<int> GetEC2SecurityGroups(ObservableCollection<SecurityGroup> col, Vpc vpc = null, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
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

        public async Task<int> GetEC2Vpcs(ObservableCollection<Vpc> col, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
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
            var _zones = new List<string>();

            // Regions that we're interested in.
            RegionEndpoint[] regions = { RegionEndpoint.USEast1, RegionEndpoint.USEast2, RegionEndpoint.USWest1, RegionEndpoint.USWest2 };

            var result = await Task.Run(async () => {
                foreach (var ep in regions) {
                    var client = new AmazonEC2Client(AccessKey, SecretKey, ep);
                    var req = new DescribeAvailabilityZonesRequest();
                    DescribeAvailabilityZonesResponse resp;

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
                }
                
                return 0;
            });

            col.Clear();
            _zones.Sort();
            _zones.ForEach(x => col.Add(x));

            return result;
        }

        public async Task<int> GetEC2InstanceStatus(ObservableCollection<InstanceStatus> col, Instance instance, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
            var _status = new List<InstanceStatus>();

            var req = new DescribeInstanceStatusRequest();
            req.InstanceIds.Add(instance.InstanceId);

            DescribeInstanceStatusResponse resp;
            var result = await Task.Run(async () => { 
                try { resp = await client.DescribeInstanceStatusAsync(req); }
                catch (AmazonEC2Exception e) {
                    throw e;
                }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeInstanceStatusAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                foreach (var s in resp.InstanceStatuses) {
                    _status.Add(s);
                }

                return 0;
            });

            col.Clear();
            _status.ForEach(x => col.Add(x));
            return result;
        }

        public async Task<int> GetEC2Subnets(ObservableCollection<Subnet> col, Vpc vpc = null, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
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

        public async Task<int> GetEC2KeyPairs(ObservableCollection<KeyPairInfo> col, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
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

         public async Task<int> GetEC2Images(ObservableCollection<Image> col, string zone = "us-east-1") {
            zone = NormalizeZoneToEndpoint(zone);
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.GetBySystemName(zone));
            var _images = new List<Image>();

            var req = new DescribeImagesRequest();
            req.Filters = new List<Filter>();
            AddImageFilter(req.Filters, "owner-id", "239734009475");
            AddImageFilter(req.Filters, "is-public", "false");

            DescribeImagesResponse resp;
            var result = await Task.Run(async () => {
                try { resp = await client.DescribeImagesAsync(req); }
                catch (AmazonEC2Exception e) { throw e; }

                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                    throw new AmazonEC2Exception($"EC2 function: DescribeSubnetsAsync() " +
                        $"failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }

                resp.Images.ForEach((x) => _images.Add(x));

                return 0;
            });

            col.Clear();
            _images.Sort((a, b) => string.Compare(a.Name, b.Name));
            _images.ForEach(x => col.Add(x));

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

        private void AddSignatureTag(RunInstancesRequest req) {
            if (req.TagSpecifications.Count == 0) {
                req.TagSpecifications.Add(new TagSpecification() {
                    ResourceType = ResourceType.Instance,
                    Tags = new List<Tag>()
                });
            }
            var taglist = req.TagSpecifications[0].Tags;

            taglist.Add(new Tag() {
                Key = "signature",
                Value = $"Myrina({Environment.UserName})"
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
                taglist.Add(tag);
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

        private void AddImageFilter(List<Filter> flist, string name, string value) {
            Filter f = new Filter();
            f.Values = new List<string>();

            f.Name = name;
            f.Values.Add(value);

            flist.Add(f);
        }
        #endregion
    }
}
