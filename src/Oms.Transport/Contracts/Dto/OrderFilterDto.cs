using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    /// <summary>What a free-text blotter filter restricts the orders to; a null member does not restrict.</summary>
    [DataContract]
    public class OrderFilterDto
    {
        [DataMember]
        public OrderDtoWay? Way { get; set; }

        [DataMember]
        public int? FundId { get; set; }

        [DataMember]
        public int? SecurityId { get; set; }

        [DataMember]
        public string Status { get; set; }
    }
}
