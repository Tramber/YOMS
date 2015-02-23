using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Messages
{
    [DataContract]
    public class ResponseBase
    {
        [DataMember]
        public int CorrelationId;

        [DataMember]
        public AcknowledgeType Acknowledge = AcknowledgeType.Success;

        [DataMember]
        public string Message;
    }
}
