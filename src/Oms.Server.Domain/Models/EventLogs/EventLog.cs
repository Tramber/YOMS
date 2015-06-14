using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.EventLogs
{
    public abstract class EventLog<TTrigger>
    {
        public EventLog()
        {
        }

        protected EventLog(ITriggerContext context, TTrigger trigger, TriggerStatus status)
        {
            Context = context;
            Trigger = trigger;
            Status = status;
        }

        public ITriggerContext Context { get; internal set; }

        public TTrigger Trigger { get; internal set; }

        public TriggerStatus Status { get; internal set; }
    }
}
