using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Oms.Client.Services.Notifications;

namespace Oms.Client.Avalonia
{
    public class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var messenger = WeakReferenceMessenger.Default;
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(messenger, new NotificationService(messenger)),
                };
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
