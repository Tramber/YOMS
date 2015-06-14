using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.EventLogs
{
    public abstract class EventLog<TTrigger>
    {
        public ITriggerContext Context { get; internal set; }

        public TTrigger Trigger { get; internal set; }
    }
}
