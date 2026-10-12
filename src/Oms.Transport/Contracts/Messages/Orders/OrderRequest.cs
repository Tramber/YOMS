using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages.Orders
{
    public class OrderRequest : Request<int, OrderStateCommand>
    {
        public OrderRequest(string token, OrderStateCommand command, List<int> itemList, TransactionMode transactionMode)
            : base(token, command, itemList, transactionMode)
        {
        }
    }
}