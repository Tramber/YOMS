using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages.Orders
{
    public class OrderMarketRequest : Request<OrderStateCommand, int>
    {
        public OrderMarketRequest(string token, int requestCommand, List<OrderStateCommand> itemList, TransactionMode transactionMode) : base(token, requestCommand, itemList, transactionMode)
        {
        }
    }
}