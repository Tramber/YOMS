using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Oms.Client.Framework.Observables;
using Oms.Client.Model;

namespace Oms.Client.Modules.OrderBlotter
{

    [Export(typeof(IOrderBlotter))]
    public class OrderBlotterViewModel : Screen, IOrderBlotter
    {
        private readonly BindableCollectionEx<OrderAdapter> _orderAdapters = new BindableCollectionEx<OrderAdapter>();
        private volatile int lastFreeGroupId = 4000;
        private string _filterText;
        private FastSearch<OrderAdapter> fastSearch = new FastSearch<OrderAdapter>();

        private readonly CollectionViewSource _orderViewSource = new CollectionViewSource();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _orderViewSource.Source = _orderAdapters;
            _orderViewSource.Filter += (sender, args) => args.Accepted = fastSearch.Predicate((OrderAdapter)args.Item);
            _orderViewSource.GroupDescriptions.Add(new PropertyGroupDescription("GroupId"));
            _orderViewSource.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
            _orderViewSource.IsLiveFilteringRequested = true;
            _orderViewSource.IsLiveGroupingRequested = true;
            _orderViewSource.IsLiveSortingRequested = true;
            CreateOrders();
        }

        public void Exit()
        {
            base.TryClose(true);
        }

        public bool CanUnGroupOrders(object arg)
        {
            var list = arg as IList;
            if (list == null) return false;
            return list.OfType<OrderAdapter>().All(o => o.GroupId != 0);;
        }

        public bool CanGroupOrders(IList list)
        {
            if (list == null) return false;
            return list.OfType<OrderAdapter>().All(o => o.GroupId == 0);
        }

        public void UnGroupOrders(object obj)
        {
            if (!CanUnGroupOrders(obj)) return;
            var list = obj as IList;
            if (list == null) return;
            var orders = list.OfType<OrderAdapter>().ToList();
            foreach (var orderAdapter in orders)
            {
                orderAdapter.GroupId = 0;
            }
            RefreshOrders(orders);
        }

        private void RefreshOrders(List<OrderAdapter> orders)
        {
            _orderAdapters.Refresh(orders);
        }

        public void GroupOrders(IList list)
        {
            if (!CanGroupOrders(list)) return;
            if (list == null) return;
            var orders = list.OfType<OrderAdapter>().ToList();
            var groupId = lastFreeGroupId++;
            foreach (var orderAdapter in orders)
            {
                orderAdapter.GroupId = groupId;
            }
            RefreshOrders(orders);
        }

        private void CreateOrders()
        {
            var random = new Random();
            const int itemCount = 50;
            for (int i = 0; i < itemCount; i++)
            {
                _orderAdapters.Add(new OrderAdapter
                {
                    Id = i,
                    Price = random.NextDouble()*100.0,
                    Quantity = random.Next(100, 100000),
                    GroupId = i % 5
                });
            }
        }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (_filterText == value) return;
                fastSearch.Clear();
                fastSearch.AddPredicate(_filterText);
                _filterText = value;
                this.NotifyOfPropertyChange(() => FilterText);

            }
        }

        public CollectionViewSource OrderViewSource
        {
            get { return _orderViewSource; }
        }
    }
}