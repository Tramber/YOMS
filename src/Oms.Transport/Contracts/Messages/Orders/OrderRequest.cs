using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages.Orders
{
    public class OrderRequest : Request<int, OrderStateCommand>
    {
        public OrderRequest(string token, OrderStateCommand requestCommand, List<int> itemList, TransactionMode transactionMode)
            : base(token, requestCommand, itemList, transactionMode)
        {
        }
    }
}