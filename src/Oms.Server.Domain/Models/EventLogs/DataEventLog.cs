using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Domain.Models.EventLogs
{
    public abstract class DataEventLog<TTrigger, TData> : EventLog<TTrigger>
    {
        public DataEventLog()
        {
        }

        public DataEventLog(ITriggerContext context, TTrigger trigger, TriggerStatus status, TData data)
            : base(context, trigger, status)
        {
            Data = data;
        } 

        public TData Data { get; internal set; }
    }
}