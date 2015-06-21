using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class TradeEventLog : EventLog<TradeTrigger, TradeState, ITrade>
    {
        internal TradeEventLog()
        {
        }

        public TradeEventLog(Interfaces.Models.ITriggerContext context, TradeTrigger trigger, TriggerStatus triggerStatus, TradeState state, ITrade trade = null)
            : base(context, trigger, triggerStatus, state, trade)
        {
        }
    }
}