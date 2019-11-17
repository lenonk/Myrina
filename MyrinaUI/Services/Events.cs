using Amazon.EC2.Model;
using System.Collections.ObjectModel;

namespace MyrinaUI.Services {
    public class AccessKeyChanged {  public string value { get; set; } }
    public class SecretKeyChanged {  public string value { get; set; } }
    public class ZoneChanged {  public string value { get; set; } }
    public class InstanceTypeChanged {  public string value { get; set; } }
    public class ImageChanged {  public string value { get; set; } }
    public class VpcChanged {  public string value { get; set; } }
    public class TagsChanged {  public ObservableCollection<Tag> value { get; set; } }
    public class KeyPairChanged {  public string value { get; set; } }
    public class SettingsChanged { }
}
