namespace Oms.Server.Domain.Interfaces.Models
{
    public interface ITradeEditableData
    {
        double ExecutionQuantity { get;}

        double ExecutionAmountNet { get; }

        TradeState TradeState { get; }

        Side Side { get; }

    }
}