using System.Collections.Generic;
using Oms.Transport.Contracts.Dto;

namespace Oms.Transport.Contracts.Messages.Orders
{
    public class OrderEditionRequest : EditionRequest<OrderDto>
    {
        public OrderEditionRequest(string token, EditionCommand command, List<OrderDto> itemList, TransactionMode transactionMode) : base(token, command, itemList, transactionMode)
        {
        }
    }
}