using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.DomainEvents
{
    public abstract class OrderDomainEvent : IDomainEvent
    {
        protected OrderDomainEvent(IOrder order)
        {
            Order = order;
        }
        public IOrder Order { get; private set; }    
    }
}