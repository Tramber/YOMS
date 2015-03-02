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
        private readonly IOrderManagementService _orderManagementService;

        public OrderServiceContract(IOrderManagementService orderManagementService)
        {
            _orderManagementService = orderManagementService;
        }

        public OrderDto[] GetOrders()
        {
            return new OrderDto[0];
        }

        public void CreateOrder(OrderDto order)
        {

        }

        public void SubscribeOrders()
        {
                var client = OperationContext.Current.GetCallbackChannel<IOrderServiceContractCallback>();
        }

        public void UnsubcribeOrders()
        {

        }
    }
}
