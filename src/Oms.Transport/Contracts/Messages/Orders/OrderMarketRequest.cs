using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages.Orders
{
    public class OrderMarketRequest : Request<int, OrderMarketCommand>
    {
        public OrderMarketRequest(string token, OrderMarketCommand command, List<int> itemList, TransactionMode transactionMode)
            : base(token, command, itemList, transactionMode)
        {
        }
    }
}