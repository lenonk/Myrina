using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace MyrinaUI.Models {
    static public class EC2UtilityModel {
        static string AccessKey = "AKIAVUTLF6PF7KZOCUQE";
        static string SecretKey = "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";

        static public async Task LaunchEC2Instance(string SAvailabilityZone, string SInstanceType, 
            string SSubnet, string SAmi) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);

            RunInstancesRequest req = new RunInstancesRequest();
            req.Placement = new Placement();
            req.Placement.AvailabilityZone = SAvailabilityZone;
            req.InstanceType = SInstanceType;
            req.SubnetId = SSubnet;
            req.ImageId = SAmi;
            req.MinCount = 1;
            req.MaxCount = 1;
            try {
                RunInstancesResponse resp = await client.RunInstancesAsync(req);
                foreach (Instance instance in resp.Reservation.Instances) {
                    MessageBox.Show($"Sucessfully started instance id: {instance.InstanceId}");
                }
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        static public async Task GetEC2InstanceTypes(ObservableCollection<string> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            List<string> _types = new List<string>();

            DescribeSpotPriceHistoryRequest req = new DescribeSpotPriceHistoryRequest();

            try {
                DescribeSpotPriceHistoryResponse resp = await client.DescribeSpotPriceHistoryAsync(req);

                foreach (SpotPrice price in resp.SpotPriceHistory) {
                    if (!_types.Contains(price.InstanceType))
                        _types.Add(price.InstanceType);
                }
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message);
            }

            _types.Sort();
            col.Clear();
            _types.ForEach(x => col.Add(x));
        }

        static public async Task GetEC2AvailabilityZones(ObservableCollection<string> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            List<string> _zones = new List<string>();

            col.Clear();

            DescribeAvailabilityZonesRequest req = new DescribeAvailabilityZonesRequest();

            try {
                DescribeAvailabilityZonesResponse resp = await client.DescribeAvailabilityZonesAsync(req);

                foreach (AvailabilityZone zone in resp.AvailabilityZones) {
                    if (!_zones.Contains(zone.ZoneName) && zone.State.ToString() == "available")
                        _zones.Add(zone.ZoneName);
                }


                _zones.Sort();
                col.Clear();
                _zones.ForEach(x => col.Add(x));
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message);
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
                                State = instance.State.Name
                            });
                        }
                    }

                    req.NextToken = resp.NextToken;
                    if (resp.NextToken == null)
                        done = true;
                }

                col.Clear();
                _instances.ForEach(x => col.Add(x));
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message);
            }
        }
        static public async Task GetEC2Subnets(ObservableCollection<EC2SubnetModel> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            List<EC2SubnetModel> _subnets = new List<EC2SubnetModel>();

            DescribeSubnetsRequest req = new DescribeSubnetsRequest();

            try {
                DescribeSubnetsResponse resp = await client.DescribeSubnetsAsync();

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
            }
            catch (AmazonEC2Exception e) {
                MessageBox.Show(e.Message);
            }
        }
    }
}
