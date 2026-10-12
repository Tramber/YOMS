using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Oms.Client.Modules.Shell;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;

namespace Oms.Client
{
    public partial class App : Application
    {
        private ServiceProvider _services;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _services = new ServiceCollection()
                .AddSingleton<IMessenger>(WeakReferenceMessenger.Default)
                .AddSingleton<INotificationService, NotificationService>()
                .AddSingleton<IWindowService, WindowService>()
                .AddSingleton<ShellViewModel>()
                .BuildServiceProvider();

            MainWindow = new MetroWindow
            {
                Title = "OMS",
                Content = new ShellView(),
                DataContext = _services.GetRequiredService<ShellViewModel>(),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _services?.Dispose();
            base.OnExit(e);
        }
    }
}
