namespace Oms.Server.Domain.Models.EventLogs
{
    public abstract class DataEventLog<TTrigger, TData> : EventLog<TTrigger>
    {
        public TData Data { get; internal set; }
    }
}