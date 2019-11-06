using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace MyrinaUI.Models {
    static public class EC2UtilityModel {
        static string AccessKey = "AKIAVUTLF6PF7KZOCUQE";
        static string SecretKey = "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";

        static public async Task GetEC2InstanceTypes(ObservableCollection<string> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            List<string> _types = new List<string>();

            DescribeSpotPriceHistoryRequest req = new DescribeSpotPriceHistoryRequest();
            DescribeSpotPriceHistoryResponse resp = await client.DescribeSpotPriceHistoryAsync(req);

            foreach (SpotPrice price in resp.SpotPriceHistory) {
                if (!_types.Contains(price.InstanceType))
                    _types.Add(price.InstanceType);
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
            DescribeAvailabilityZonesResponse resp = await client.DescribeAvailabilityZonesAsync(req);

            foreach (AvailabilityZone zone in resp.AvailabilityZones) {
                if (!_zones.Contains(zone.ZoneName) && zone.State.ToString() == "available")
                    _zones.Add(zone.ZoneName);
            }

            _zones.Sort();
            col.Clear();
            _zones.ForEach(x => col.Add(x));
        }

        static public async Task GetEC2Instances(ObservableCollection<EC2InstanceModel> col) {
            var client = new AmazonEC2Client(AccessKey, SecretKey, RegionEndpoint.USEast1);
            bool done = false;

            col.Clear();
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
                        col.Add(new EC2InstanceModel {
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
        }
    }
}
