using System;
using System.Collections.Generic;
using System.Text;

namespace MyrinaUI.Models {
    public class EC2SubnetModel {
        public string AvailabiltyZone { get; set; }
        public string CidrBlock { get; set; }
        public string OwnerId { get; set; }
        public string SubnetId { get; set; }
        public string VpcId { get; set; }
    }
}
