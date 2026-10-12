using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Oms.Client.Model;
using Oms.Client.Modules.Notification;
using Oms.Client.Modules.OrderBlotter;
using Oms.Client.Modules.OrderEditor;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;

namespace Oms.Client.Modules.Shell
{
    /// <summary>Main window: hosts the documents as tabs, the login and connection to the server, the notification overlay and the order editor.</summary>
    public partial class ShellViewModel : ObservableObject, IRecipient<INotification>
    {
        private readonly IWindowService _windowService;
        private readonly INotificationService _notificationService;
        private readonly IOrderServiceProxy _proxy;

        [ObservableProperty]
        private DocumentViewModel _activeDocument;

        [ObservableProperty]
        private string _connectionStatus = "Not connected";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateOrderCommand))]
        private bool _isConnected;

        public ShellViewModel(IWindowService windowService, INotificationService notificationService, IMessenger messenger,
            IOrderServiceProxy proxy, Session session)
        {
            _windowService = windowService;
            _notificationService = notificationService;
            _proxy = proxy;
            Session = session;
            Blotter = new OrderBlotterViewModel(proxy, notificationService, session, EditOrderAsync);
            Documents = [Blotter];
            ActiveDocument = Blotter;
            Notifications = [];
            messenger.RegisterAll(this);
        }

        public Session Session { get; }

        public OrderBlotterViewModel Blotter { get; }

        public ObservableCollection<DocumentViewModel> Documents { get; }

        /// <summary>Notifications shown in the overlay. They are never removed.</summary>
        public ObservableCollection<NotificationViewModel> Notifications { get; }

        /// <summary>Connects to the server and loads the orders. The blotter then follows the changes pushed by the server.</summary>
        [RelayCommand]
        private async Task ConnectAsync()
        {
            if (IsConnected) return;
            ConnectionStatus = "Connecting...";
            try
            {
                await _proxy.ConnectAsync();
                await Blotter.LoadAsync();
                IsConnected = true;
                ConnectionStatus = "Connected as " + Session.Login;
            }
            catch (Exception ex)
            {
                ConnectionStatus = "Not connected";
                _notificationService.ProcessError("Connection failed", ex.Message);
            }
        }

        [RelayCommand(CanExecute = nameof(IsConnected))]
        private Task CreateOrderAsync() => OpenEditorAsync(null);

        /// <summary>Opens the order editor on an existing order (the blotter's Edit button).</summary>
        public Task EditOrderAsync(OrderAdapter order) => OpenEditorAsync(order);

        private async Task OpenEditorAsync(OrderAdapter order)
        {
            var editor = new OrderEditorViewModel(_proxy, Session);
            try
            {
                await editor.LoadAsync(order);
            }
            catch (Exception ex)
            {
                _notificationService.ProcessError("Order editor", ex.Message);
                return;
            }
            await _windowService.ShowDialogAsync(editor);
        }

        /// <summary>Adds each notification sent on the messenger to the overlay.</summary>
        public void Receive(INotification message)
        {
            Notifications.Add(new NotificationViewModel(message));
        }
    }
}
