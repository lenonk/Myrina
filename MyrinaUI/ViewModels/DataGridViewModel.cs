using Amazon.EC2;
using Amazon.EC2.Model;
using Avalonia.Threading;
using MyrinaUI.Utility;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace MyrinaUI.ViewModels {
    public class DataGridViewModel : ViewModelBase {
        public ObservableCollection<Instance> EC2Instances { get; } = new ObservableCollection<Instance>();
        private DispatcherTimer _refreshTimer = new DispatcherTimer();

        public DataGridViewModel() {
            RefreshEC2Instances();

            _refreshTimer.Interval = TimeSpan.FromSeconds(30);
            _refreshTimer.Tick += (sender, e) => { RefreshEC2Instances(); };
            //_refreshTimer.Start();
        }

        private Instance _sInstance;
        public Instance SInstance {
            get { return _sInstance; }
            set { this.RaiseAndSetIfChanged(ref _sInstance, value); }
        }

        public async void RefreshEC2Instances() {
            Instance si = SInstance;
            await EC2Utility.GetEC2Instances(EC2Instances)
                .ContinueWith(_ => ResetSelectedInstance(si));
        }

        private void ResetSelectedInstance(Instance si) {
            if (si == null) return;

            foreach (Instance instance in EC2Instances) {
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
                switch (code) {
                    case AmazonCommand.Reboot:
                        msg = await EC2Utility.RebootEC2Instance(SInstance.InstanceId);
                        LogViewModel.LogView.Log(msg);
                        break;
                    case AmazonCommand.Start:
                        msg = await EC2Utility.StartEC2Instance(SInstance.InstanceId);
                        LogViewModel.LogView.Log(msg);
                        break;
                    case AmazonCommand.Stop:
                        msg = await EC2Utility.StopEC2Instance(SInstance.InstanceId);
                        LogViewModel.LogView.Log(msg);
                        break;
                    case AmazonCommand.Terminate:
                        msg = await EC2Utility.TerminateEC2Instance(SInstance.InstanceId);
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
