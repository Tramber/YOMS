using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.DomainEvents
{
    public class BookingDomainEvent : TradeDomainEvent
    {
        public BookingDomainEvent(ITrade trade) : base(trade)
        {
        }
    }
}