using Amazon.EC2.Model;
using MyrinaUI.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace MyrinaUI.ViewModels {
    public class InstanceInfoViewModel : ViewModelBase {

        private ObservableCollection<InstanceStatus> _status = new ObservableCollection<InstanceStatus>();
        public ObservableCollection<InstanceStatus> Status {
            get { return _status; }
            set { this.RaiseAndSetIfChanged(ref _status, value); }
        } 

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
            EventSystem.Subscribe<Instance>((x) => { SInstance = x; });

            this.WhenAnyValue(x => x.SInstance)
                .Where(x => x != null)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => CheckInstanceStatus(x));
        }

        public async void CheckInstanceStatus(Instance instance) {
            await EC2Service.Instance.GetEC2InstanceStatus(Status, instance);
        }

        public void Collapse() => IsOpen = false;
        public void Expand() => IsOpen = true;
    }
}
