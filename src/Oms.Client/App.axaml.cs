using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Oms.Client.Modules.Shell;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;

namespace Oms.Client
{
    public class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new ServiceCollection()
                    .AddSingleton<IMessenger>(WeakReferenceMessenger.Default)
                    .AddSingleton<INotificationService, NotificationService>()
                    .AddSingleton<IWindowService, WindowService>()
                    .AddSingleton<ShellViewModel>()
                    .BuildServiceProvider();

                desktop.MainWindow = new Window
                {
                    Title = "OMS",
                    Width = 900,
                    Height = 600,
                    Content = new ShellView(),
                    DataContext = services.GetRequiredService<ShellViewModel>(),
                };
                desktop.Exit += (_, _) => services.Dispose();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
