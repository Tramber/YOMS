using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class OrderParameterEventLog<TParam> : OrderEventLog
    {
        internal OrderParameterEventLog()
        {
        }

        public OrderParameterEventLog(ITriggerContext context, OrderStateMachine.Trigger trigger,
            TriggerStatus triggerStatus, OrderStateMachine.State state, IOrder order, TParam parameters)
            : base(context, trigger, triggerStatus, state, order)
        {
            Parameters = parameters;
        }

        public TParam Parameters { get; internal set; }
    }
}