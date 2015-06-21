using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.DomainEvents
{
    public class AcceptanceDomainEvent : OrderDomainEvent
    {
        public AcceptanceDomainEvent(IOrder order) : base(order)
        {
        }
    }
}