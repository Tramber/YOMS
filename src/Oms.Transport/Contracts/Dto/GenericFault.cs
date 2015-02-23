using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public class GenericFault
    {
        public GenericFault(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public string Operation { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
