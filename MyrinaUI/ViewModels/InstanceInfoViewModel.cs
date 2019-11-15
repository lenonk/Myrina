using Amazon.EC2.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyrinaUI.ViewModels {
    public class InstanceInfoViewModel : ViewModelBase {

        private Instance _sInstance;
        public Instance SInstance {
            get { return _sInstance; }
            set { this.RaiseAndSetIfChanged(ref _sInstance, value); }
        }

        public InstanceInfoViewModel() {
            Instance a = new Instance();
            a.InstanceId = "foo";
            a.ImageId = "bar";

            SInstance = a;
        }
    }
}
