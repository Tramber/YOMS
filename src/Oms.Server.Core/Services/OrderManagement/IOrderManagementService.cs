using System.Collections.Generic;
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
    }
}
