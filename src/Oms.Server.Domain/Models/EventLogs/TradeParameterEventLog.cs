using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class TradeParameterEventLog<TParam> : TradeEventLog
    {
        internal TradeParameterEventLog()
        {
        }

        public TradeParameterEventLog(ITriggerContext context, TradeTrigger trigger,
            TriggerStatus triggerStatus, TradeState state, ITrade trade, TParam parameters)
            : base(context, trigger, triggerStatus, state, trade)
        {
            Parameters = parameters;
        }

        public TParam Parameters { get; internal set; }
    }
}