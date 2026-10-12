using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public enum CurrencyDto
    {
        [EnumMember]
        Usd,
        [EnumMember]
        Eur,
        [EnumMember]
        Gbp,
        [EnumMember]
        Undefined,
    }
}