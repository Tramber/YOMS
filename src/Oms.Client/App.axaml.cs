using System;
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
                    .AddSingleton<Session>()
                    // OMS_SERVER overrides the default address of the server.
                    .AddSingleton<IOrderServiceProxy>(_ => new OrderServiceProxy(Environment.GetEnvironmentVariable("OMS_SERVER") ?? "http://localhost:9001"))
                    .AddSingleton<ShellViewModel>()
                    .BuildServiceProvider();

                desktop.MainWindow = new Window
                {
                    Title = "OMS",
                    Width = 1200,
                    Height = 720,
                    MinWidth = 900,
                    MinHeight = 520,
                    Content = new ShellView(),
                    DataContext = services.GetRequiredService<ShellViewModel>(),
                };
                // OrderServiceProxy is IAsyncDisposable only, so the provider must be disposed asynchronously.
                desktop.Exit += (_, _) => services.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
