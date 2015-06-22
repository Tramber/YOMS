using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages.Orders
{
    public class OrderMarketRequest : Request<int, OrderStateCommand>
    {
        public OrderMarketRequest(string token, OrderStateCommand requestCommand, List<int> itemList, TransactionMode transactionMode)
            : base(token, requestCommand, itemList, transactionMode)
        {
        }
    }
}