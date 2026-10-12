using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;
using System;

namespace Oms.Transport.Contracts
{
    public interface IOrderServiceContract
    {
        EditionResponse<OrderDto> Handle(OrderEditionRequest request);

        Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderRequest request);

        Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderMarketRequest request);
    }

    /// <summary>Names of the SignalR hub methods shared by server and client.</summary>
    public static class OrderHubMethods
    {
        public const string Path = "/orders";
        public const string Subscribe = "SubscribeToEvents";
        public const string Unsubscribe = "UnsubscribeToEvents";
        public const string OrderListUpdated = "OrderListUpdated";
    }
}
