using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages.Orders
{
    public class OrderRequest : Request<OrderStateCommand, int>
    {
        public OrderRequest(string token, int requestCommand, List<OrderStateCommand> itemList, TransactionMode transactionMode) : base(token, requestCommand, itemList, transactionMode)
        {
        }
    }
}