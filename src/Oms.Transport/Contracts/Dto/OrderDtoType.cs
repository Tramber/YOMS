using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public enum OrderDtoType
    {
        [EnumMember]
        Market,

        [EnumMember]
        Limit,

        [EnumMember]
        Stop
    }
}