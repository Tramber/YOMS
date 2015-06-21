namespace Oms.Server.Domain.Interfaces.Models
{
    public enum TradeTrigger
    {
        Undefined,
        Create,
        Cancel,
        BookingSend,
        Booked,
        ForceBooked,
        BookingRefresh,
        BookingFailed,
    }
}