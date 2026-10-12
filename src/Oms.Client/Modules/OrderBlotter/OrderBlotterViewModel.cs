using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Oms.Client.Framework.Observables;
using Oms.Client.Model;
using Oms.Client.Modules.Shell;
using Oms.Client.Services;
using Oms.Client.Services.Notifications;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Client.Modules.OrderBlotter
{
    /// <summary>
    /// Grid of the orders of the server, grouped by <c>GroupId</c> (a local grouping) and sortable. It is filled by
    /// <see cref="LoadAsync"/>, follows the changes pushed by the server, and sends the commands of the toolbar for the selected orders.
    /// </summary>
    public partial class OrderBlotterViewModel : DocumentViewModel
    {
        private readonly IOrderServiceProxy _proxy;
        private readonly INotificationService _notifications;
        private readonly Session _session;
        private readonly ObservableCollection<OrderAdapter> _orders = new ObservableCollection<OrderAdapter>();
        private readonly FastSearch<OrderAdapter> _fastSearch = new FastSearch<OrderAdapter>();
        private readonly Func<OrderAdapter, Task> _editOrder;
        private int _lastFreeGroupId = 4000;

        [ObservableProperty]
        private string _filterText = "";

        /// <param name="editOrder">Opens the editor on an order (the Edit button); without it Edit does nothing.</param>
        public OrderBlotterViewModel(IOrderServiceProxy proxy, INotificationService notifications, Session session, Func<OrderAdapter, Task> editOrder = null)
        {
            _editOrder = editOrder;
            _proxy = proxy;
            _notifications = notifications;
            _session = session;

            OrdersView = new DataGridCollectionView(_orders) { Filter = item => _fastSearch.Predicate((OrderAdapter)item) };
            OrdersView.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(OrderAdapter.GroupId)));
            OrdersView.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(OrderAdapter.Id)));

            // Pushed changes arrive on a background thread.
            proxy.Subscribe(new ChangeObserver(change => Dispatcher.UIThread.Post(() => Apply(change.Item1, change.Item2))));
        }

        public override string DisplayName => "Order Blotter";

        public DataGridCollectionView OrdersView { get; }

        /// <summary>Accepts <c>Property = "value"</c> (nested paths allowed, e.g. <c>Security.IsinCode = "X"</c>); anything else shows all rows.</summary>
        partial void OnFilterTextChanged(string value)
        {
            _fastSearch.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    _fastSearch.AddPredicate(value);
                }
                catch (Exception)
                {
                    // Incomplete or invalid expression while typing: keep showing everything.
                    _fastSearch.Clear();
                }
            }
            OrdersView.Refresh();
        }

        /// <summary>Replaces the rows by the orders of the server. The local groups of the orders that stay are kept.</summary>
        public async Task LoadAsync()
        {
            var dtos = await _proxy.GetOrdersAsync();
            var groups = _orders.ToDictionary(o => o.Id, o => o.GroupId);
            _orders.Clear();
            foreach (var dto in dtos)
            {
                var adapter = OrderAdapter.From(dto);
                int groupId;
                if (groups.TryGetValue(dto.Id, out groupId))
                    adapter.GroupId = groupId;
                _orders.Add(adapter);
            }
        }

        /// <summary>Applies a change pushed by the server: add or update the orders, or remove the deleted ones. Call it on the UI thread.</summary>
        public void Apply(ItemOperationType operationType, OrderDto[] dtos)
        {
            foreach (var dto in dtos)
            {
                var existing = _orders.FirstOrDefault(o => o.Id == dto.Id);
                if (operationType == ItemOperationType.Delete || operationType == ItemOperationType.Remove)
                {
                    if (existing != null) _orders.Remove(existing);
                }
                else if (existing != null)
                {
                    existing.UpdateFrom(dto);
                }
                else
                {
                    _orders.Add(OrderAdapter.From(dto));
                }
            }
            OrdersView.Refresh();
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

        /// <summary>Opens the editor on the selected order; the server decides whether its state allows the update.</summary>
        [RelayCommand]
        private async Task EditOrderAsync(IList selected)
        {
            var orders = Orders(selected);
            if (orders.Count == 0 || _editOrder == null) return;
            if (orders.Count > 1)
            {
                _notifications.ProcessError("Edit", "Select a single order to edit");
                return;
            }
            await _editOrder(orders[0]);
        }

        [RelayCommand]
        private Task CancelOrdersAsync(IList selected) => SendStateAsync("Cancel", OrderStateCommand.Cancel, selected);

        [RelayCommand]
        private Task RecallOrdersAsync(IList selected) => SendStateAsync("Recall", OrderStateCommand.Recall, selected);

        [RelayCommand]
        private Task DeleteOrdersAsync(IList selected) => SendStateAsync("Delete", OrderStateCommand.Delete, selected);

        [RelayCommand]
        private Task AcceptPendingAsync(IList selected) => SendStateAsync("Accept", OrderStateCommand.AcceptPending, selected);

        [RelayCommand]
        private Task RejectPendingAsync(IList selected) => SendStateAsync("Reject", OrderStateCommand.RejectPending, selected);

        [RelayCommand]
        private Task StartBookingAsync(IList selected) => SendStateAsync("Start booking", OrderStateCommand.StartBooking, selected);

        /// <summary>Executes what remains of the selected orders in one trade, at the limit price (0 for an order without one).</summary>
        [RelayCommand]
        private Task ExecuteOrdersAsync(IList selected)
        {
            var trades = Orders(selected).Where(o => o.RemainingQuantity > 0).Select(o => new TradeDto
            {
                OrderId = o.Id,
                Way = o.Way,
                ExecutionQuantity = o.RemainingQuantity,
                ExecutionAmountNet = o.RemainingQuantity * o.Price,
            }).ToList();
            return SendTradesAsync("Execute", OrderTradeCommand.AddTrade, trades);
        }

        /// <summary>Books the executions of the selected orders that are not booked yet.</summary>
        [RelayCommand]
        private Task BookTradesAsync(IList selected)
        {
            var trades = Orders(selected)
                .SelectMany(o => o.Trades.Where(t => t.State == "New" || t.State == "BookingSent" || t.State == "BookingError"))
                .ToList();
            return SendTradesAsync("Book trades", OrderTradeCommand.BookTrade, trades);
        }

        [RelayCommand]
        private Task SendToMarketAsync(IList selected) => SendMarketAsync("Send to market", OrderMarketCommand.MarketSend, selected);

        [RelayCommand]
        private Task CancelAtMarketAsync(IList selected) => SendMarketAsync("Cancel at market", OrderMarketCommand.MarketCancel, selected);

        private async Task SendStateAsync(string label, OrderStateCommand command, IList selected)
        {
            var ids = SelectedIds(selected);
            if (ids.Count == 0) return;
            try
            {
                var response = await _proxy.SendAsync(new OrderRequest(_session.Login, command, ids, default(TransactionMode)));
                Report(label, response.ResultList);
            }
            catch (Exception ex)
            {
                _notifications.ProcessError(label, ex.Message);
            }
        }

        private async Task SendMarketAsync(string label, OrderMarketCommand command, IList selected)
        {
            var ids = SelectedIds(selected);
            if (ids.Count == 0) return;
            try
            {
                var response = await _proxy.SendAsync(new OrderMarketRequest(_session.Login, command, ids, default(TransactionMode)));
                Report(label, response.ResultList);
            }
            catch (Exception ex)
            {
                _notifications.ProcessError(label, ex.Message);
            }
        }

        private async Task SendTradesAsync(string label, OrderTradeCommand command, List<TradeDto> trades)
        {
            if (trades.Count == 0) return;
            try
            {
                var response = await _proxy.SendAsync(new OrderTradeRequest(_session.Login, command, trades, default(TransactionMode)));
                Report(label, response.ResultList);
            }
            catch (Exception ex)
            {
                _notifications.ProcessError(label, ex.Message);
            }
        }

        private static List<OrderAdapter> Orders(IList selected)
        {
            return selected == null ? new List<OrderAdapter>() : selected.OfType<OrderAdapter>().ToList();
        }

        private static List<int> SelectedIds(IList selected)
        {
            return selected == null ? new List<int>() : selected.OfType<OrderAdapter>().Select(o => o.Id).ToList();
        }

        /// <summary>A refused order is shown as an error; the accepted ones show up through the changes pushed by the server.</summary>
        private void Report(string label, IEnumerable<Tuple<int, GenericResultDto>> results)
        {
            foreach (var result in results.Where(r => r.Item2.ErrorCode != 0))
                _notifications.ProcessError(label + ": order " + result.Item1, result.Item2.ErrorMessage);
        }

        private sealed class ChangeObserver : IObserver<Tuple<ItemOperationType, OrderDto[]>>
        {
            private readonly Action<Tuple<ItemOperationType, OrderDto[]>> _onChange;
            public ChangeObserver(Action<Tuple<ItemOperationType, OrderDto[]>> onChange) { _onChange = onChange; }
            public void OnNext(Tuple<ItemOperationType, OrderDto[]> value) => _onChange(value);
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }
    }
}
