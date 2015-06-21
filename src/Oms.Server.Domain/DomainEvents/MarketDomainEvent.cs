using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.DomainEvents
{
    public class MarketDomainEvent : OrderDomainEvent
    {
        public MarketDomainEvent(IOrder order, MarketRouteType marketRoute, MarketOperation marketOperation)
            : base(order)
        {
            MarketRoute = marketRoute;
            MarketOperation = marketOperation;
        }

        MarketRouteType MarketRoute { get; set; }

        MarketOperation MarketOperation { get; set; }
    }
}