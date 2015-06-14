namespace Oms.Server.Domain.Models.Orders
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
        OverFilled
    }
}