using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public enum CurrencyDto
    {
        [EnumMember]
        USD,
        [EnumMember]
        EUR,
        [EnumMember]
        GBP,
    }
}