using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.DomainEvents
{
    public interface IDomainEvent
    {
    }

    public abstract class TradeDomainEvent
    {
        protected TradeDomainEvent(ITrade trade)
        {
            Trade = trade;
        }

        public ITrade Trade { get; private set; }
    }
}
