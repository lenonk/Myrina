using Avalonia;
using Avalonia.Markup.Xaml;

namespace MyrinaUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
