using System;
using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public class AssetDto
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string IsinCode { get; set; }

        [DataMember]
        public string BloombergTicker { get; set; }

        [DataMember]
        public CurrencyDto Currency { get; set; }

        [DataMember]
        public AssetDtoType Type { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        [DataMember]
        public UserDto Creator { get; set; }

        [DataMember]
        public AssetOriginDtoType Origin { get; set; }
    }
}
