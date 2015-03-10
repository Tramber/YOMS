using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public enum OrderDtoWay
    {
        [EnumMember]
        Buy,
        [EnumMember]
        Sell
    }
}