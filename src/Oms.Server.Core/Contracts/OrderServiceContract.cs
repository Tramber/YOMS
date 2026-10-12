using System;
using System.Collections.Generic;
using Oms.Server.Core.Services;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Contracts
{
    /// <summary>Server implementation of the order service. Work in progress: the responses are not built yet.</summary>
    public class OrderServiceContract(IOrderManagementService orderManagementService) : IOrderServiceContract
    {
        /// <summary>Raised when orders change; the SignalR host forwards it to subscribed clients.</summary>
        public event Action<ItemOperationType, OrderDto[]> OrderListUpdated;

        public EditionResponse<OrderDto> Handle(OrderEditionRequest request)
        {
            var response = orderManagementService.HandleRequest(request);
            throw new NotImplementedException();
        }

        public Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderRequest request)
        {
            var response = orderManagementService.HandleRequest(request);
            return null;
        }

        public Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderMarketRequest request)
        {
            var response = orderManagementService.HandleRequest(request);
            return null;
        }

        private void SendOrderChange(ItemOperationType operationType, List<OrderDto> orderDtoList)
        {
            OrderListUpdated?.Invoke(operationType, orderDtoList.ToArray());
        }
    }
}
