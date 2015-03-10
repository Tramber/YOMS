using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core.Services
{
    public class OrderManagementService : IOrderManagementService
    {
        private volatile int lastOrderIdGenerated = 1;
        private readonly ConcurrentDictionary<int, OrderDto> orderDtoCache = new ConcurrentDictionary<int, OrderDto>();


        public Task<ItemOperationResponse<OrderDto>> CreateOrders(List<OrderDto> orderDtoList)
        {
            var ordersToAdd = orderDtoList.Where(o => o != null).ToList();
            foreach (var order in ordersToAdd)
            {
                order.Id = lastOrderIdGenerated++;
                orderDtoCache.TryAdd(order.Id, order);
            }
            return Task.FromResult(new ItemOperationResponse<OrderDto>(true, ItemOperationType.Add, ordersToAdd));
        }

        public Task<ItemOperationResponse<OrderDto>> CancelOrders(List<int> orderIdList)
        {
            var ordersToRemove = new List<OrderDto>();
            foreach (var orderId in orderIdList)
            {
                OrderDto orderDto;
                if (orderDtoCache.TryRemove(orderId, out orderDto))
                {
                    ordersToRemove.Add(orderDto);
                }
            }
            return Task.FromResult(new ItemOperationResponse<OrderDto>(true, ItemOperationType.Add, ordersToRemove));
        }

        public Task<ItemOperationResponse<OrderDto>> UpdateOrders(List<OrderDto> orderDtoList)
        {
            Contract.Requires(orderDtoList != null);
            var ordersToUpdate = orderDtoList.Where(o => o != null)
                .Where(orderDto => orderDtoCache.TryUpdate(orderDto.Id, orderDto, orderDto))
                .ToList();
            return Task.FromResult(new ItemOperationResponse<OrderDto>(true, ItemOperationType.Add, ordersToUpdate));
        }
    }
}
