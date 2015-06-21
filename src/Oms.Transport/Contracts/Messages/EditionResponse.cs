using System.Collections.Generic;

namespace Oms.Transport.Contracts.Messages
{
    public class EditionResponse<TResult> : Response<TResult, EditionCommand>
    {
        public EditionResponse(EditionCommand command, List<TResult> resultList)
            : base(command, resultList)
        {
        }
    }
}