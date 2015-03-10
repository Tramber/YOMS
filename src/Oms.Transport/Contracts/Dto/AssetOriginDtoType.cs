using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public enum AssetOriginDtoType
    {
        [EnumMember]
        User,
        [EnumMember]
        Sophis
    }
}