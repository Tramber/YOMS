using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Oms.Client.Model;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;

namespace Oms.Client.Avalonia
{
    public record NotificationItem(string Text);

    public partial class MainWindowViewModel : ObservableObject, IRecipient<INotification>
    {
        private readonly INotificationService _notificationService;
        private readonly ObservableCollection<OrderAdapter> _orders = new ObservableCollection<OrderAdapter>();
        private int _lastFreeGroupId = 4000;
        private OrderServiceProxy _proxy;

        [ObservableProperty]
        private string _filterText = "";

        [ObservableProperty]
        private string _connectionStatus = "Not connected";

        public MainWindowViewModel(IMessenger messenger, INotificationService notificationService)
        {
            _notificationService = notificationService;
            messenger.RegisterAll(this);

            var random = new Random();
            for (var i = 0; i < 50; i++)
                _orders.Add(new OrderAdapter
                {
                    Id = i,
                    Price = random.NextDouble() * 100.0,
                    Quantity = random.Next(100, 100000),
                    GroupId = i % 5,
                });

            OrdersView = new DataGridCollectionView(_orders) { Filter = MatchesFilter };
            OrdersView.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(OrderAdapter.GroupId)));
            OrdersView.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(OrderAdapter.Id)));
        }

        public DataGridCollectionView OrdersView { get; }

        public ObservableCollection<NotificationItem> Notifications { get; } = new ObservableCollection<NotificationItem>();

        partial void OnFilterTextChanged(string value) => OrdersView.Refresh();

        private bool MatchesFilter(object item)
        {
            var o = (OrderAdapter)item;
            return string.IsNullOrWhiteSpace(FilterText)
                || $"{o.Id} {o.GroupId} {o.Quantity} {o.Price:N2}".Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }

        [RelayCommand]
        private void GroupOrders(IList selected)
        {
            var orders = selected?.OfType<OrderAdapter>().ToList();
            if (orders == null || orders.Count == 0 || orders.Any(o => o.GroupId != 0)) return;
            var groupId = _lastFreeGroupId++;
            foreach (var order in orders) order.GroupId = groupId;
            OrdersView.Refresh();
        }

        [RelayCommand]
        private void UnGroupOrders(IList selected)
        {
            var orders = selected?.OfType<OrderAdapter>().ToList();
            if (orders == null || orders.Count == 0 || orders.Any(o => o.GroupId == 0)) return;
            foreach (var order in orders) order.GroupId = 0;
            OrdersView.Refresh();
        }

        [RelayCommand]
        private void CreateNotification() => _notificationService.ProcessInfo("hello", "Ding Ding Dong");

        public void Receive(INotification message) => Notifications.Add(new NotificationItem($"{message.Title}: {message.Description}"));

        [RelayCommand]
        private async System.Threading.Tasks.Task ConnectAsync()
        {
            if (_proxy != null) return;
            ConnectionStatus = "Connecting...";
            try
            {
                _proxy = new OrderServiceProxy();
                _proxy.Subscribe(new OrderObserver(this));
                await _proxy.StartAsync();
                ConnectionStatus = "Connected to http://localhost:9001/orders";
            }
            catch (Exception ex)
            {
                _proxy = null;
                ConnectionStatus = "Connection failed: " + ex.Message;
            }
        }

        private void AddOrders(Oms.Transport.Contracts.Dto.OrderDto[] dtos)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var dto in dtos)
                    _orders.Add(new OrderAdapter { Id = dto.Id, Price = dto.PriceLimit ?? 0, Quantity = dto.Quantity });
            });
        }

        private sealed class OrderObserver : IObserver<Tuple<Oms.Transport.Contracts.ItemOperationType, Oms.Transport.Contracts.Dto.OrderDto[]>>
        {
            private readonly MainWindowViewModel _owner;
            public OrderObserver(MainWindowViewModel owner) { _owner = owner; }
            public void OnNext(Tuple<Oms.Transport.Contracts.ItemOperationType, Oms.Transport.Contracts.Dto.OrderDto[]> value) => _owner.AddOrders(value.Item2);
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }
    }
}
