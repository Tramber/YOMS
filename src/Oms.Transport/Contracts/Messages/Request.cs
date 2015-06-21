using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages
{
    public abstract class Request<TItem, TCommand>
    {
        protected Request(string token, TCommand command, List<TItem> itemList, TransactionMode transactionMode)
        {
            ItemList = itemList;
            TransactionMode = transactionMode;
            Command = command;
            TransactionMode = transactionMode;
        }

        public List<TItem> ItemList { get; private set; }

        public TransactionMode TransactionMode { get; private set; }

        public string Token { get; set; }

        public TCommand Command { get; set; }
    }
}