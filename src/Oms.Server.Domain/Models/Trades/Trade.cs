using System.Collections.Generic;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;

namespace Oms.Server.Domain.Models.Trades
{
    /// <summary>
    /// An execution on an order. Every change is recorded as a <see cref="TradeEventLog"/>.
    /// Only the Create and Cancel triggers are implemented; the booking triggers are not.
    /// </summary>
    public class Trade : ITrade
    {
        public int Id { get; set; }

        public IList<TradeEventLog> EventLogs { get; set; } = new List<TradeEventLog>();

        public double ExecutionQuantity { get; set; }

        public double ExecutionAmountNet { get; set; }

        public TradeState TradeState { get; set; }

        public Side Side { get; set; }

        public IOrder Order { get; set; }

        public GenericResult Create(ITriggerContext context, ITradeEditableData editableData)
        {
            if (TradeState != TradeState.Undefined)
                return GenericResult.FailureFormat("The trade is already created ({0})", TradeState);
            if (editableData == null)
                return GenericResult.Failure("The trade data are required");
            if (editableData.ExecutionQuantity <= 0)
                return GenericResult.Failure("The execution quantity must be positive");

            ExecutionQuantity = editableData.ExecutionQuantity;
            ExecutionAmountNet = editableData.ExecutionAmountNet;
            Side = editableData.Side;
            TradeState = TradeState.New;
            EventLogs.Add(new TradeParameterEventLog<ITradeEditableData>(
                context, TradeTrigger.Create, TriggerStatus.Done, TradeState, this, editableData));
            return GenericResult.Success();
        }

        public GenericResult Cancel(ITriggerContext context)
        {
            if (TradeState == TradeState.Undefined)
                return GenericResult.Failure("The trade is not created yet");
            if (TradeState == TradeState.Cancelled)
                return GenericResult.Failure("The trade is already cancelled");

            TradeState = TradeState.Cancelled;
            EventLogs.Add(new TradeEventLog(context, TradeTrigger.Cancel, TriggerStatus.Done, TradeState, this));
            return GenericResult.Success();
        }
    }
}
