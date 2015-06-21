using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;

namespace Oms.Server.Domain.Models.EventLogs
{
    public abstract class EventLog<TTrigger, TTState, TEntity> : IIdentifiable
    {
        internal EventLog()
        {
        }

        protected EventLog(ITriggerContext context, TTrigger trigger, TriggerStatus triggerStatus, TTState state, TEntity entity)
        {
            Context = context;
            Trigger = trigger;
            TriggerStatus = triggerStatus;
            State = state;
            Entity = entity;
        }

        public ITriggerContext Context { get; internal set; }

        public TTrigger Trigger { get; internal set; }

        public TriggerStatus TriggerStatus { get; internal set; }

        public TTState State { get; internal set; }

        public int Id { get; set; }

        public TEntity Entity { get; internal set; }
    }
}
