namespace Oms.Server.Domain.Interfaces.Models
{
    public enum OrderStatus
    {
        Undefined,
        New,
        PartiallyFilled,
        Filled,
        Cancelled,
        CancelledFilled,
        Pending,
        OverFilled,
        CancelPending,
        UpdatePending
    }
}