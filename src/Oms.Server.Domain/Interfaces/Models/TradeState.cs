namespace Oms.Server.Domain.Interfaces.Models
{
    public enum TradeState
    {
        Undefined,
        New,
        Booking,
        BookingSent,
        Booked,
        Cancelled,
        BookingError,
    }
}