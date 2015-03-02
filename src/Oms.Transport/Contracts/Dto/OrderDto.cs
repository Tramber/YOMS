using System;
using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    [DataContract]
    public class OrderDto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public double Quantity { get; set; }

        [DataMember]
        public double Price { get; set; }

        [DataMember]
        public OrderDtoWay Way { get; set; }

        [DataMember]
        public AssetDto Asset { get; set; }

        [DataMember]
        public UserDto Owner { get; set; }

        [DataMember]
        public UserDto Creator { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        [DataMember]
        public OrderDtoType OrderType { get; set; }

        [DataMember]
        public OrderDtoValidityType Validity { get; set; }

        [DataMember]
        public DateTime? ExpiryDate { get; set; }
    }
}

