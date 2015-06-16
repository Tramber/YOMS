namespace Oms.Server.Domain.Models.Trades
{
    public enum TradeStatus
    {
        Undefined,
        Cancelled,
        New,
        Booked,
        BookingSending,
        BookingSent,
    }
}