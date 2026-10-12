using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Oms.Client.Framework.Observables;
using Oms.Client.Model;
using Oms.Client.Modules.Shell;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Modules.OrderBlotter
{
    /// <summary>Grid of orders, grouped by <c>GroupId</c> and sortable. Starts with sample data until the server pushes real orders.</summary>
    public partial class OrderBlotterViewModel : DocumentViewModel
    {
        private readonly ObservableCollection<OrderAdapter> _orders = new ObservableCollection<OrderAdapter>();
        private readonly FastSearch<OrderAdapter> _fastSearch = new FastSearch<OrderAdapter>();
        private int _lastFreeGroupId = 4000;

        [ObservableProperty]
        private string _filterText = "";

        public OrderBlotterViewModel()
        {
            // Sample data until the server pushes real orders.
            var random = new Random();
            for (var i = 0; i < 50; i++)
                _orders.Add(new OrderAdapter
                {
                    Id = i,
                    Price = random.NextDouble() * 100.0,
                    Quantity = random.Next(100, 100000),
                    GroupId = i % 5,
                });

            OrdersView = new DataGridCollectionView(_orders) { Filter = item => _fastSearch.Predicate((OrderAdapter)item) };
            OrdersView.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(OrderAdapter.GroupId)));
            OrdersView.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(OrderAdapter.Id)));
        }

        public override string DisplayName => "Order Blotter";

        /// <summary>The grouped, sorted and filtered rows bound to the grid.</summary>
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

        /// <summary>Adds the orders pushed by the server. Call it on the UI thread.</summary>
        public void AddOrders(OrderDto[] dtos)
        {
            foreach (var dto in dtos)
                _orders.Add(new OrderAdapter { Id = dto.Id, Price = dto.PriceLimit ?? 0, Quantity = dto.Quantity });
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
    }
}
