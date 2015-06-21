using System.Collections.Generic;
using Oms.Transport.Contracts.Dto;

namespace Oms.Transport.Contracts.Messages.Trades
{
    public class TradeEditionRequest : EditionRequest<TradeDto>
    {
        public TradeEditionRequest(string token, EditionCommand command, List<TradeDto> itemList, TransactionMode transactionMode)
            : base(token, command, itemList, transactionMode)
        {
        }
    }
}