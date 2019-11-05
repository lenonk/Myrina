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
    }
}
