using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyrinaUI.Models {
    public class EC2InstanceModel {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string AvailabilityZone { get; set; }
        public string State { get; set; }

        public Instance EC2Instance { get; set; }
        public List<TagDescription> Tags;
    }
}
