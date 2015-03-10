using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public class GenericFault
    {
        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public string Operation { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
