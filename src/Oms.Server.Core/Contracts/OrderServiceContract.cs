using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Oms.Framework.Threading;
using Oms.Server.Core.Services;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;

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

        public OrderDto[] GetOrders()
        {
            return new OrderDto[0];
        }

        public async void CreateOrders(OrderDto[] orderDtoList)
        {
            var operationResponse = await _orderManagementService.CreateOrders(orderDtoList.ToList());
            await sendTaskQueue.AddTask(() => SendOrderChange(operationResponse.OperationType, operationResponse.ItemList));
        }

        public async void CancelOrders(int[] orderIdList)
        {
            var operationResponse = await _orderManagementService.CancelOrders(orderIdList.ToList());
            await sendTaskQueue.AddTask(() => SendOrderChange(operationResponse.OperationType, operationResponse.ItemList));
        }

        public async void UpdateOrders(OrderDto[] orderDtoList)
        {
            var operationResponse = await _orderManagementService.UpdateOrders(orderDtoList.ToList());
            await sendTaskQueue.AddTask(() => SendOrderChange(operationResponse.OperationType, operationResponse.ItemList));
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
