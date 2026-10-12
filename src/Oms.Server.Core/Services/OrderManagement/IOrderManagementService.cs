using Oms.Server.Domain.Framework;
using Oms.Transport.Contracts.Messages.Orders;

namespace Oms.Server.Core.Services
{
    public interface IOrderManagementService
    {
        GenericResult HandleRequest(OrderEditionRequest request);
        GenericResult HandleRequest(OrderMarketRequest request);
        GenericResult HandleRequest(OrderRequest request);
    }
}