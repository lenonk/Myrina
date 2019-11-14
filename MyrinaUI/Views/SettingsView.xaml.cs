using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyrinaUI.Views {
    public class SettingsView : UserControl {
        public SettingsView() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        public void Show() {
            IsVisible = true;
            Opacity = 1;
            ZIndex = 99;
        }

        public void Hide() {
            Opacity = 0;
            ZIndex = -1;
            IsVisible = false;
        }

        /*public async Task<int> SlideIn(TimeSpan time) {
            var result = await Task.Run(() => {
                Double stepsPerMilli = Margin.Left / time.TotalMilliseconds;
                DateTime last = DateTime.Now;

                while (Margin.Left > 0) {
                    TimeSpan duration = DateTime.Now - last;

                    if (duration.TotalMilliseconds >= 1) {
                        var steps = (stepsPerMilli * duration.TotalMilliseconds);
                        var newMargin = Math.Max(0, Math.Round(Margin.Left - steps));
                        SetValue(MarginProperty, new Thickness(newMargin, 0, 0, 0));
                        Debug.WriteLine($"Moving left to {newMargin}");
                    }

                    Debug.WriteLine($"Last: {last.ToString()}, Now: {DateTime.Now.ToString()}, spm: {stepsPerMilli}, duration: {duration.TotalMilliseconds}");
                    last = DateTime.Now;
                }

                return 0;
            });

            return result;
        }*/
    }
}
