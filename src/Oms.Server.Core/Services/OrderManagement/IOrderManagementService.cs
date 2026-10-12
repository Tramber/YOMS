using System.Collections.Generic;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Services
{
    /// <summary>
    /// Applies the commands of a request to its orders. Each returns one <see cref="OrderCommandResult"/> per order of the
    /// request, in the same order; an order that fails does not stop the others.
    /// </summary>
    public interface IOrderManagementService
    {
        IList<OrderCommandResult> HandleRequest(OrderEditionRequest request);
        IList<OrderCommandResult> HandleRequest(OrderMarketRequest request);
        IList<OrderCommandResult> HandleRequest(OrderRequest request);

        /// <summary>Adds, cancels or books executions of orders, one result per trade of the request.</summary>
        IList<OrderCommandResult> HandleRequest(OrderTradeRequest request);

        /// <summary>Every order, with its event log.</summary>
        IList<Order> GetOrders();

        /// <summary>The active securities an order can be placed on.</summary>
        IList<Security> GetSecurities();

        IList<IFund> GetFunds();
    }
}
