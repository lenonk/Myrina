using Amazon.EC2.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MyrinaUI.ViewModels {
    public class InstanceInfoViewModel : ViewModelBase {

        private bool _isOpen = true;
        public bool IsOpen {
            get { return _isOpen; }
            set { this.RaiseAndSetIfChanged(ref _isOpen, value); }
        }

        private Instance _sInstance;
        public Instance SInstance {
            get { return _sInstance; }
            set { this.RaiseAndSetIfChanged(ref _sInstance, value); }
        }

        public InstanceInfoViewModel() {
            EventSystem.Subscribe<SelectedInstanceChanged>((x) => { SInstance = x.SInstance; });
        }

        public void Collapse() => IsOpen = false;
        public void Expand() => IsOpen = true;
    }
}
