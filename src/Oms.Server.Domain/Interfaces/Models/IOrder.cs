using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IOrder : IOrderCoreData, IOrderEditableData, IOrderRoutingData, IIdentifiable
    {
    }
}