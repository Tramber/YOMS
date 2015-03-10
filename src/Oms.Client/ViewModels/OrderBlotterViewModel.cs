using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Oms.Client.Framework.Observables;
using Oms.Client.Model;

namespace Oms.Client.ViewModels
{

    [Export(typeof(IOrderBlotter))]
    public class OrderBlotterViewModel : Screen, IOrderBlotter
    {
        private readonly BindableCollectionEx<OrderAdapter> _orderAdapters = new BindableCollectionEx<OrderAdapter>();
        private volatile int lastFreeGroupId = 4000;

        protected override void OnInitialize()
        {
            base.OnInitialize();
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
            OrderAdapters.Refresh(orders);
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

        public BindableCollectionEx<OrderAdapter> OrderAdapters
        {
            get { return _orderAdapters; }
        }
    }
}