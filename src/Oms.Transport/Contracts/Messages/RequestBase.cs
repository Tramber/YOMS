using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Messages
{
    [DataContract]
    public class RequestBase
    {
        [DataMember]
        public int RequestId;

        [DataMember]
        public OperationType Operation;
    }
}
