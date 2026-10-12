namespace Oms.Transport.Contracts.Messages.Orders
{
    public enum OrderTradeCommand
    {
        /// <summary>Records an execution on the order.</summary>
        AddTrade,
        /// <summary>Cancels an execution of the order.</summary>
        CancelTrade,
        /// <summary>Marks an execution as booked; the order terminates once all its executions are booked.</summary>
        BookTrade
    }
}
