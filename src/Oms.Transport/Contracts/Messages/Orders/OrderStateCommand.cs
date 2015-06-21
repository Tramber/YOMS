namespace Oms.Transport.Contracts.Messages.Orders
{
    public enum OrderStateCommand
    {
        Cancel,
        AcceptPending,
        StartBooking,
        RejectPending,
        Recall,
        Delete
    }
}