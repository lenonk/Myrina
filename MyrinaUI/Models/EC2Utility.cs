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

namespace MyrinaUI.Models {
    static public class EC2Utility {
        static string AccessKey = "AKIAVUTLF6PF7KZOCUQE";
        static string SecretKey = "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";

        static public async Task LaunchEC2Instance(string SAvailabilityZone, string SInstanceType, 
            string SSubnet, string SAmi, List<Tag> tags = null) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);

            RunInstancesRequest req = new RunInstancesRequest();
            req.Placement = new Placement();
            req.Placement.AvailabilityZone = SAvailabilityZone;
            req.InstanceType = SInstanceType;
            req.SubnetId = SSubnet;
            req.ImageId = SAmi;
            req.MinCount = 1;
            req.MaxCount = 1;
            req.TagSpecifications = new List<TagSpecification>();

            req.TagSpecifications.Add(new TagSpecification() {
                ResourceType = ResourceType.Instance,
                Tags = new List<Tag>()
            });

            req.TagSpecifications[0].Tags.Add(new Tag() { Key = "mgr", Value = "jeff-zawadski" });
            req.TagSpecifications[0].Tags.Add(new Tag() { Key = "Name", Value = "Created by Myrina" });
            if (tags != null) {
                // TODO:  Add other tags here, but make sure it has a name
            }

            try {
                RunInstancesResponse resp = await client.RunInstancesAsync(req);
                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    foreach (Instance instance in resp.Reservation.Instances) {
                        MessageBox.Show($"Sucessfully started instance id: {instance.InstanceId}", MainWindowViewModel.MainWindow);
                    }

                    return;
                }

                MessageBox.Show($"EC2 function: RunInstancesAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainWindowViewModel.MainWindow);
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
                    MessageBox.Show($"Sucessfully requested termination of instance id: {instance}", MainWindowViewModel.MainWindow);
                    return;
                }

                MessageBox.Show($"EC2 function: TerminateInstancesAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainWindowViewModel.MainWindow);
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
                    MessageBox.Show($"Sucessfully requested start of instance id: {instance}", MainWindowViewModel.MainWindow);
                    return;
                }

                MessageBox.Show($"EC2 function: StartInstancesAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainWindowViewModel.MainWindow);
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
                    MessageBox.Show($"Sucessfully requested stop of instance id: {instance}", MainWindowViewModel.MainWindow);
                    return;
                }

                MessageBox.Show($"EC2 function: StopInstancesAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainWindowViewModel.MainWindow);
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
                    MessageBox.Show($"Sucessfully requested reboot of instance id: {instance}", MainWindowViewModel.MainWindow);
                    return;
                }

                MessageBox.Show($"EC2 function: RebootInstancesAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainWindowViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }

        static public async Task GetEC2InstanceTypes(ObservableCollection<string> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            List<string> _types = new List<string>();

            DescribeSpotPriceHistoryRequest req = new DescribeSpotPriceHistoryRequest();

            try {
                DescribeSpotPriceHistoryResponse resp = await client.DescribeSpotPriceHistoryAsync(req);

                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    foreach (SpotPrice price in resp.SpotPriceHistory) {
                        if (!_types.Contains(price.InstanceType))
                            _types.Add(price.InstanceType);
                    }

                    _types.Sort();
                    col.Clear();
                    _types.ForEach(x => col.Add(x));

                    return;
                }

                MessageBox.Show($"EC2 function: DescribeSpotPriceHistoryAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainWindowViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }

        static public async Task GetEC2AvailabilityZones(ObservableCollection<string> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            List<string> _zones = new List<string>();

            DescribeAvailabilityZonesRequest req = new DescribeAvailabilityZonesRequest();

            try {
                DescribeAvailabilityZonesResponse resp = await client.DescribeAvailabilityZonesAsync(req);

                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    foreach (AvailabilityZone zone in resp.AvailabilityZones) {
                        if (!_zones.Contains(zone.ZoneName) && zone.State.ToString() == "available")
                            _zones.Add(zone.ZoneName);
                    }

                    _zones.Sort();
                    col.Clear();
                    _zones.ForEach(x => col.Add(x));

                    return;
                }

                MessageBox.Show($"EC2 function: DescribeAvailabilityZonesAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainWindowViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }

        static public async Task GetEC2InstanceTags(EC2InstanceModel instance) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            List<EC2InstanceModel> _instances = new List<EC2InstanceModel>();

            var req = new DescribeTagsRequest();
            req.Filters.Add(new Filter() { 
                Name = "resource-id", 
                Values = new List<string> { 
                    instance.Id 
                } 
            });

            try {
                var resp = await client.DescribeTagsAsync(req);

                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    instance.Tags = resp.Tags;
                    return;
                }

                MessageBox.Show($"EC2 function: DescribeTagsAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]", MainWindowViewModel.MainWindow);
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }
        
        static public async Task GetEC2Instances(ObservableCollection<EC2InstanceModel> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            List<EC2InstanceModel> _instances = new List<EC2InstanceModel>();
            bool done = false;

            try {
                DescribeInstancesRequest req = new DescribeInstancesRequest();
                while (!done) {
                    DescribeInstancesResponse resp = await client.DescribeInstancesAsync(req);

                    if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                        foreach (Reservation res in resp.Reservations) {
                            foreach (Instance instance in res.Instances) {
                                string name = "";
                                foreach (var tag in instance.Tags) {
                                    if (tag.Key == "Name")
                                        name = tag.Value;
                                }
                                _instances.Add(new EC2InstanceModel {
                                    Name = name,
                                    Id = instance.InstanceId,
                                    Type = instance.InstanceType,
                                    AvailabilityZone = instance.Placement.AvailabilityZone,
                                    State = instance.State.Name,
                                    EC2Instance = instance,
                                });

                                _ = GetEC2InstanceTags(_instances[_instances.Count - 1]);
                            }
                        }

                        req.NextToken = resp.NextToken;
                        if (resp.NextToken == null)
                            done = true;

                        col.Clear();
                        _instances.ForEach(x => col.Add(x));

                        return;
                    }

                    MessageBox.Show($"EC2 function: DescribeInstanceAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
                }
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }

        static public async Task GetEC2Subnets(ObservableCollection<EC2SubnetModel> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            List<EC2SubnetModel> _subnets = new List<EC2SubnetModel>();

            DescribeSubnetsRequest req = new DescribeSubnetsRequest();

            try {
                DescribeSubnetsResponse resp = await client.DescribeSubnetsAsync();

                if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                    foreach (Subnet subnet in resp.Subnets) {
                        _subnets.Add(new EC2SubnetModel {
                            AvailabiltyZone = subnet.AvailabilityZone,
                            CidrBlock = subnet.CidrBlock,
                            OwnerId = subnet.OwnerId,
                            SubnetId = subnet.SubnetId,
                            VpcId = subnet.VpcId
                        });
                    }

                    col.Clear();
                    _subnets.ForEach(x => col.Add(x));

                    return;
                }

                    MessageBox.Show($"EC2 function: DescribeSubnetsAsync() failed with HTTP error: [{resp.HttpStatusCode.ToString()}]");
            }
            catch (AmazonEC2Exception e) {
                throw e;
            }
        }
    }
}
