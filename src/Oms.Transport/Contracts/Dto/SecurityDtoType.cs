using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public enum SecurityDtoType
    {
        [EnumMember]
        Equity,
        [EnumMember]
        Bond,
        [EnumMember]
        Future
    }
}