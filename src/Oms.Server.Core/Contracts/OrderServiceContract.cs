using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Oms.Framework.Threading;
using Oms.Server.Core.Services;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Contracts
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class OrderServiceContract : IOrderServiceContract
    {
        private readonly IOrderManagementService _orderManagementService;
        private readonly BlockingCollection<IOrderServiceContractCallback> contractCallbacks = new BlockingCollection<IOrderServiceContractCallback>();
        private readonly TaskDispatcherQueue sendTaskQueue = new TaskDispatcherQueue(1);

        public OrderServiceContract(IOrderManagementService orderManagementService)
        {
            _orderManagementService = orderManagementService;
        }

        public EditionResponse<OrderDto> Handle(OrderEditionRequest request)
        {
            var response = _orderManagementService.HandleRequest(request);
            return null;
            //sendTaskQueue.AddTask(() => SendOrderChange(ItemOperationType.Add, response.));
        }

        public Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderRequest request)
        {
            var response = _orderManagementService.HandleRequest(request);
            return null;
            //sendTaskQueue.AddTask(() => SendOrderChange(ItemOperationType.Add, response.));
        }

        public Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderMarketRequest request)
        {
            var response = _orderManagementService.HandleRequest(request);
            return null;
            //sendTaskQueue.AddTask(() => SendOrderChange(ItemOperationType.Add, response.));
        }

        public void SubscribeToEvents()
        {
            var client = OperationContext.Current.GetCallbackChannel<IOrderServiceContractCallback>();
            contractCallbacks.TryAdd(client);

        }

        public void UnsubscribeToEvents()
        {
            var client = OperationContext.Current.GetCallbackChannel<IOrderServiceContractCallback>();
            contractCallbacks.TryTake(out client);
        }

        private void SendOrderChange(ItemOperationType operationType, List<OrderDto> orderDtoList)
        {
            foreach (var orderServiceContractCallback in contractCallbacks)
            {
                orderServiceContractCallback.OrderListUpdated(operationType, orderDtoList.ToArray());
            }
        }
    }


          
}
