using System.Runtime.Serialization;
using Oms.Transport.Contracts.Dto;

namespace Oms.Transport.Contracts.Messages
{
    [DataContract]
    public class UserRequest
    {
        [DataMember]
        public UserDto[] Users { get; set; }
    }
}
