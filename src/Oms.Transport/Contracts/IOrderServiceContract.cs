using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;
using System;
using System.Collections.Generic;

namespace Oms.Transport.Contracts
{
    /// <summary>Operations a client can ask the server for. Shared by the server implementation and the hub.</summary>
    public interface IOrderServiceContract
    {
        /// <summary>Creates, stashes or updates orders. One result per submitted order, in the same order: the order as saved (or as submitted when it failed) and the outcome.</summary>
        Response<Tuple<OrderDto, GenericResultDto>, EditionCommand> Handle(OrderEditionRequest request);

        /// <summary>Applies a state command (cancel, recall, delete, ...) to existing orders. One outcome per order id.</summary>
        Response<Tuple<int, GenericResultDto>, OrderStateCommand> Handle(OrderRequest request);

        /// <summary>Sends orders to the market or cancels them there. One outcome per order id.</summary>
        Response<Tuple<int, GenericResultDto>, OrderMarketCommand> Handle(OrderMarketRequest request);

        /// <summary>Every order, to fill a blotter; changes are then pushed as OrderListUpdated events.</summary>
        List<OrderDto> GetOrders();

        /// <summary>The securities an order can be placed on.</summary>
        List<SecurityDto> GetSecurities();

        List<FundDto> GetFunds();
    }

    /// <summary>Names of the SignalR hub methods shared by server and client.</summary>
    public static class OrderHubMethods
    {
        public const string Path = "/orders";
        public const string HandleOrderEdition = "HandleOrderEdition";
        public const string HandleOrder = "HandleOrder";
        public const string HandleOrderMarket = "HandleOrderMarket";
        public const string GetOrders = "GetOrders";
        public const string GetSecurities = "GetSecurities";
        public const string GetFunds = "GetFunds";
        public const string Subscribe = "SubscribeToEvents";
        public const string Unsubscribe = "UnsubscribeToEvents";
        public const string OrderListUpdated = "OrderListUpdated";
    }
}
