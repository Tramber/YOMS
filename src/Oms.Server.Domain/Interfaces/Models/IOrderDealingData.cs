namespace Oms.Server.Domain.Interfaces.Models
{

    public interface IOrderRoutingData
    {
        
    }

    public interface IOrderDealingData : IOrderRoutingData
    {
        ITrade Trade { get; }
    }
}
