using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Oms.Client.Modules.Notification;
using Oms.Client.Modules.OrderBlotter;
using Oms.Client.Modules.OrderEditor;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;

namespace Oms.Client.Modules.Shell
{
    public partial class ShellViewModel : ObservableObject, IRecipient<INotification>
    {
        private readonly IWindowService _windowService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private object _activeDocument;

        public ShellViewModel(IWindowService windowService, INotificationService notificationService, IMessenger messenger)
        {
            _windowService = windowService;
            _notificationService = notificationService;
            var blotter = new OrderBlotterViewModel();
            Documents = new ObservableCollection<object> { blotter };
            ActiveDocument = blotter;
            Notifications = new ObservableCollection<NotificationViewModel>();
            messenger.RegisterAll(this);
        }

        public ObservableCollection<object> Documents { get; }

        public ObservableCollection<NotificationViewModel> Notifications { get; }

        [RelayCommand]
        private void CreateOrder()
        {
            _windowService.ShowDialog(new OrderEditorViewModel());
        }

        [RelayCommand]
        private void CreateNotification()
        {
            _notificationService.ProcessInfo("hello", "Ding Ding Dong");
        }

        public void Receive(INotification message)
        {
            Notifications.Add(new NotificationViewModel(message));
        }
    }
}
