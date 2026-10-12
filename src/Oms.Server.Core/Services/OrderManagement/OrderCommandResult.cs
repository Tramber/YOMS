using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Models.Orders;

namespace Oms.Server.Core.Services
{
    /// <summary>Outcome of one command on one order of a request.</summary>
    public class OrderCommandResult(int orderId, Order order, GenericResult result)
    {
        /// <summary>Id of the order; the id the order was saved with when it was just created.</summary>
        public int OrderId { get; private set; } = orderId;

        /// <summary>The order the command was applied to; null when it could not be loaded.</summary>
        public Order Order { get; private set; } = order;

        public GenericResult Result { get; private set; } = result;
    }
}
