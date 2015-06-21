using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.DomainEvents
{
    public class ValidationDomainEvent : OrderDomainEvent
    {
        public ValidationDomainEvent(IOrder order) : base(order)
        {
        }
    }
}