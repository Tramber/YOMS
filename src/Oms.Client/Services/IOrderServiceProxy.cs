using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oms.Transport.Contracts;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Client.Services
{
    /// <summary>
    /// The server's order service as the client sees it. Observe it to receive the order changes pushed by the server
    /// (add, update, delete); the other methods are calls. A call throws when the server cannot be reached.
    /// </summary>
    public interface IOrderServiceProxy : IObservable<Tuple<ItemOperationType, OrderDto[]>>, IAsyncDisposable
    {
        bool IsConnected { get; }

        /// <summary>Connects to the server and subscribes to the order changes.</summary>
        Task ConnectAsync();

        Task<List<OrderDto>> GetOrdersAsync();

        Task<List<SecurityDto>> GetSecuritiesAsync();

        Task<List<FundDto>> GetFundsAsync();

        /// <summary>Asks the server what a free-text filter restricts the orders to; null when the server cannot interpret it.</summary>
        Task<OrderFilterDto> InterpretFilterAsync(string text);

        /// <summary>Creates, stashes or updates orders: one outcome per submitted order.</summary>
        Task<Response<Tuple<OrderDto, GenericResultDto>, EditionCommand>> SendAsync(OrderEditionRequest request);

        /// <summary>Applies a state command to existing orders: one outcome per order id.</summary>
        Task<Response<Tuple<int, GenericResultDto>, OrderStateCommand>> SendAsync(OrderRequest request);

        /// <summary>Sends orders to the market or cancels them there: one outcome per order id.</summary>
        Task<Response<Tuple<int, GenericResultDto>, OrderMarketCommand>> SendAsync(OrderMarketRequest request);

        /// <summary>Adds, cancels or books the executions of orders: one outcome per submitted trade, keyed by its order id.</summary>
        Task<Response<Tuple<int, GenericResultDto>, OrderTradeCommand>> SendAsync(OrderTradeRequest request);
    }
}
