using Amazon.EC2;
using Amazon.EC2.Model;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using MyrinaUI.Services;
using MyrinaUI.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace MyrinaUI.ViewModels {
    public class DataGridViewModel : ViewModelBase {
        private DispatcherTimer _refreshTimer = new DispatcherTimer();

        private SourceList<Instance> _sourceInstances = new SourceList<Instance>();
        private ReadOnlyObservableCollection<Instance> _instances;
        public ReadOnlyObservableCollection<Instance> Instances {
            get { return _instances; }
            set { this.RaiseAndSetIfChanged(ref _instances, value); }
        }

        // Property for the selected instance, or row
        private Instance _sInstance;
        public Instance SInstance {
            get { return _sInstance; }
            set { this.RaiseAndSetIfChanged(ref _sInstance, value); }
        }

        private string _searchText;
        public string SearchText {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public DataGridViewModel() {
            EventSystem.Subscribe<SettingsChanged>((x) => { RefreshEC2Instances(); });

            this.WhenAnyValue(x => x.SInstance).Subscribe((x) => EventSystem.Publish(x));

            var dynamicFilter = this.WhenAnyValue(x => x.SearchText)
                .Where(x => x == null || (x.Length == 0 || x.Length >= 3))
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(text => (Func<Instance, bool>) (i => ApplyFilter(i, text)));

            _sourceInstances.Connect()
                .Filter(dynamicFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _instances)
                .Subscribe();

            RefreshEC2Instances();

            _refreshTimer.Interval = TimeSpan.FromSeconds(30);
            _refreshTimer.Tick += (sender, e) => { RefreshEC2Instances(); };
            _refreshTimer.Start();
        }

        private bool ApplyFilter(Instance i, string filter) {
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            foreach (var tag in i.Tags) {
                if (tag.Key.StartsWith(filter)) return true;
                if (tag.Value.StartsWith(filter)) return true;
            }

            if (i.InstanceId.StartsWith(filter)) return true;
            if (i.InstanceType.Value.StartsWith(filter)) return true;
            if (i.Placement.AvailabilityZone.StartsWith(filter)) return true;
            if (i.State.Name.ToString().StartsWith(filter)) return true;

            return false;
        }

        private async void RefreshEC2Instances() {
            if (string.IsNullOrWhiteSpace(Settings.Current.AccessKey) ||
                string.IsNullOrWhiteSpace(Settings.Current.SecretKey))
                return;

            Instance si = SInstance;
            await EC2Service.Instance.GetEC2Instances(_sourceInstances)
                .ContinueWith(_ => ResetSelectedInstance(si));
        }

        private void ResetSelectedInstance(Instance si) {
            if (si == null) return;

            foreach (Instance instance in _instances) {
                if (instance.InstanceId == si.InstanceId)
                    SInstance = instance;
            }
        }

        // EC2 Command methods
        private enum AmazonCommand {
            Reboot,
            Start,
            Stop,
            Terminate
        }

        private async void RunAmazonCommand(AmazonCommand code) {
            try {
                _refreshTimer.Stop();
                _refreshTimer.IsEnabled = false;

                if (SInstance == null)
                    return;

                string msg;
                var items = ViewFinder.Get<DataGridView>().FindControl<DataGrid>("instanceGrid").SelectedItems;
                switch (code) {
                    case AmazonCommand.Reboot:
                        msg = await EC2Service.Instance.RebootEC2Instance(items);
                        LogViewModel.LogView.Log(msg);
                        break;
                    case AmazonCommand.Start:
                        msg = await EC2Service.Instance.StartEC2Instance(items);
                        LogViewModel.LogView.Log(msg);
                        break;
                    case AmazonCommand.Stop:
                        msg = await EC2Service.Instance.StopEC2Instance(items);
                        LogViewModel.LogView.Log(msg);
                        break;
                    case AmazonCommand.Terminate:
                        msg = await EC2Service.Instance.TerminateEC2Instance(items);
                        LogViewModel.LogView.Log(msg);
                        break;
                    default:
                        break;
                }

                _refreshTimer.Start();
                _refreshTimer.IsEnabled = true;
            } catch (AmazonEC2Exception e) {
                LogViewModel.LogView.Log(e.Message);
            }
        }

        public void TerminateEC2Instance() => RunAmazonCommand(AmazonCommand.Terminate);
        public void StartEC2Instance() => RunAmazonCommand(AmazonCommand.Start);
        public void StopEC2Instance() => RunAmazonCommand(AmazonCommand.Stop);
        public void RebootEC2Instance() => RunAmazonCommand(AmazonCommand.Reboot);
    }
}
