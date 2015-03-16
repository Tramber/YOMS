using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Oms.Client.PO;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using IOrderServiceContractCallback = Oms.Client.PO.IOrderServiceContractCallback;

namespace Oms.Client.Services
{
    public class OrderServiceProxy
    {
        readonly PO.OrderServiceContractClient contractProxy;
        private OrderServiceContractCallback callbackProxy;
        public OrderServiceProxy()
        {
            callbackProxy = new OrderServiceContractCallback();
            contractProxy = new OrderServiceContractClient(new InstanceContext(callbackProxy), "NetTcpBinding_IOrderServiceContract");
        }

        private void Start()
        {
            contractProxy.Open();
        }

        private void Stop()
        {
            contractProxy.Close();
        }



        public async void MyTest()
        {

           // callbackProxy.Do(a => Debug.Assert(false)).Subscribe();

            Start();
            await contractProxy.SubscribeToEventsAsync();

            await contractProxy.CreateOrdersAsync(new[] {new OrderDto {Quantity = 666}});

            await contractProxy.UnsubscribeToEventsAsync();

            Stop();
        }


        public class OrderServiceContractCallback : IOrderServiceContractCallback, IObservable<Tuple<ItemOperationType, OrderDto[]>>
        {
            private IObserver<Tuple<ItemOperationType, OrderDto[]>> _observer;

            public void OrderListUpdated(ItemOperationType operationType, OrderDto[] orderDtoList)
            {
                if(_observer != null)
                    _observer.OnNext(new Tuple<ItemOperationType, OrderDto[]>(operationType, orderDtoList));
            }

            public IDisposable Subscribe(IObserver<Tuple<ItemOperationType, OrderDto[]>> observer)
            {
                _observer = observer;
                return Disposable.Empty;
            }
        }

    }
}
