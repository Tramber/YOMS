using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages
{
    public class EditionRequest<TItem> : Request<TItem, EditionCommand>
    {
        protected EditionRequest(string token, EditionCommand command, List<TItem> itemList, TransactionMode transactionMode) : base(token, command, itemList, transactionMode)
        {
        }
    }
}