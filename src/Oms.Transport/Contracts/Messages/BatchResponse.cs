using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages
{
    public class Response<TItem, TCommand>
    {
        public Response(TCommand command, List<TItem> resultList)
        {
            ResultList = resultList;
            Command = command;
        }
        public TCommand Command { get; set; }

        public List<TItem> ResultList { get; set; }
    }
}