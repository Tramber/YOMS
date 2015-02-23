using System.Runtime.Serialization;
using Oms.Transport.Contracts.Dto;

namespace Oms.Transport.Contracts.Messages
{
    [DataContract]
    public class OrderRequest : RequestBase
    {
        public OrderDto[] Orders { get; set; }
    }
}
