using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public class OrderGroupDto
    {
        [DataMember]
        public int Id { get; set; }
    }
}