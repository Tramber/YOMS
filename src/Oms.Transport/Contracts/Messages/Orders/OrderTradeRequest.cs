using System.Collections.Generic;
using Oms.Transport.Contracts.Dto;

namespace Oms.Transport.Contracts.Messages.Orders
{
    public class OrderTradeRequest : Request<TradeDto, OrderTradeCommand>
    {
        public OrderTradeRequest(string token, OrderTradeCommand command, List<TradeDto> itemList, TransactionMode transactionMode)
            : base(token, command, itemList, transactionMode)
        {
        }
    }
}
