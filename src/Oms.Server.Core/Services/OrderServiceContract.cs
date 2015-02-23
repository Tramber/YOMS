using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class OrderServiceContract : IOrderServiceContract
    {
        public OrderDto[] GetOrders()
        {
            return new OrderDto[0];
        }

        public void CreateOrder(OrderDto order)
        {

        }

        bool stopIO;  //Session State
        public void SubscribeOrders()
        {
            try
            {
                stopIO = false;
                var random = new Random();
                var client = OperationContext.Current.GetCallbackChannel<IOrderServiceContractCallback>();
                var obs = Observable.Interval(TimeSpan.FromMilliseconds(1)).TakeWhile(x => !stopIO);
                using (obs.Subscribe(x =>
                {
                    if (client != null)
                        client.Orders_OnNext(new OrderDto
                        {
                            Id = random.Next(100000),
                            Price = random.NextDouble()*10,
                            Quantity = random.NextDouble()*10000
                        });
                }, ex => Console.WriteLine(ex)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void UnsubcribeOrders()
        {
            stopIO = true;
        }
    }
}
