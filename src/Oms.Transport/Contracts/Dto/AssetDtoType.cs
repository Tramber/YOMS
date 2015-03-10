using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public enum AssetDtoType
    {
        [EnumMember]
        Equity,
        [EnumMember]
        Bond,
        [EnumMember]
        Future
    }
}