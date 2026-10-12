using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Oms.Client.Modules.Notification;
using Oms.Client.Modules.OrderBlotter;
using Oms.Client.Modules.OrderEditor;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Modules.Shell
{
    /// <summary>Main window: hosts the documents as tabs, the toolbar commands, the notification overlay and the server connection.</summary>
    public partial class ShellViewModel : ObservableObject, IRecipient<INotification>
    {
        private readonly IWindowService _windowService;
        private readonly INotificationService _notificationService;
        private readonly OrderBlotterViewModel _blotter = new OrderBlotterViewModel();
        private OrderServiceProxy _proxy;

        [ObservableProperty]
        private DocumentViewModel _activeDocument;

        [ObservableProperty]
        private string _connectionStatus = "Not connected";

        public ShellViewModel(IWindowService windowService, INotificationService notificationService, IMessenger messenger)
        {
            _windowService = windowService;
            _notificationService = notificationService;
            Documents = new ObservableCollection<DocumentViewModel> { _blotter };
            ActiveDocument = _blotter;
            Notifications = new ObservableCollection<NotificationViewModel>();
            messenger.RegisterAll(this);
        }

        /// <summary>Open tabs.</summary>
        public ObservableCollection<DocumentViewModel> Documents { get; }

        /// <summary>Notifications shown in the overlay. They are never removed.</summary>
        public ObservableCollection<NotificationViewModel> Notifications { get; }

        [RelayCommand]
        private Task CreateOrderAsync() => _windowService.ShowDialogAsync(new OrderEditorViewModel());

        [RelayCommand]
        private void CreateNotification() => _notificationService.ProcessInfo("hello", "Ding Ding Dong");

        [RelayCommand]
        private async Task ConnectAsync()
        {
            if (_proxy != null) return;
            ConnectionStatus = "Connecting...";
            try
            {
                _proxy = new OrderServiceProxy();
                _proxy.Subscribe(new OrderObserver(orders => Dispatcher.UIThread.Post(() => _blotter.AddOrders(orders))));
                await _proxy.StartAsync();
                ConnectionStatus = "Connected to http://localhost:9001/orders";
            }
            catch (Exception ex)
            {
                _proxy = null;
                ConnectionStatus = "Connection failed: " + ex.Message;
            }
        }

        /// <summary>Adds each notification sent on the messenger to the overlay.</summary>
        public void Receive(INotification message)
        {
            Notifications.Add(new NotificationViewModel(message));
        }

        private sealed class OrderObserver : IObserver<Tuple<ItemOperationType, OrderDto[]>>
        {
            private readonly Action<OrderDto[]> _onOrders;
            public OrderObserver(Action<OrderDto[]> onOrders) { _onOrders = onOrders; }
            public void OnNext(Tuple<ItemOperationType, OrderDto[]> value) => _onOrders(value.Item2);
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }
    }
}
