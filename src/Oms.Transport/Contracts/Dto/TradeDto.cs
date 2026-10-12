using System.Runtime.Serialization;

namespace Oms.Transport.Contracts.Dto
{
    /// <summary>An execution on an order. To add one, set <c>OrderId</c>, <c>Way</c>, <c>ExecutionQuantity</c> and <c>ExecutionAmountNet</c>; to cancel or book one, set <c>OrderId</c> and <c>Id</c>.</summary>
    [DataContract]
    public class TradeDto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int OrderId { get; set; }

        [DataMember]
        public OrderDtoWay Way { get; set; }

        [DataMember]
        public double ExecutionQuantity { get; set; }

        [DataMember]
        public double ExecutionAmountNet { get; set; }

        /// <summary>New, BookingSent, Booked, Cancelled, BookingError. Set by the server.</summary>
        [DataMember]
        public string State { get; set; }
    }
}
