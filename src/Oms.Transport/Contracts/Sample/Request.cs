using System.Collections.Generic;
using System.Runtime.Serialization;
using Oms.Transport.Contracts.Dto;

namespace Oms.Transport.Contracts
{
    [DataContract]
    [KnownType(typeof(CreateOrdersRequest))]
    [KnownType(typeof(UpdateOrdersRequest))]
    [KnownType(typeof(DeleteOrdersRequest))]
    [KnownType(typeof(SearchOrdersRequest))]
    public abstract class Request
    {
    }

    public class DeleteOrdersRequest
    {
        public IList<int> OrderIds { get; set; } 
    }

    public class SearchOrdersRequest
    {

    }

    public class UpdateOrdersRequest
    {
        public IList<OrderDto> Orders { get; set; } 
    }

    public class CreateOrdersRequest : Request, IRequest<CreateOrdersResponse>
    {
        public IList<OrderDto> Orders { get; set; } 
    }

    public class CreateOrdersResponse
    {
    }
}
