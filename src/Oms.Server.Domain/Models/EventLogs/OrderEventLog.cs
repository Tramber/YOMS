using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class OrderEventLog : EventLog<OrderStateMachine.Trigger, OrderStateMachine.State, IOrder>
    {
        internal OrderEventLog()
        {
        }

        public OrderEventLog(Interfaces.Models.ITriggerContext context, OrderStateMachine.Trigger trigger, TriggerStatus triggerStatus, OrderStateMachine.State state, IOrder order = null)
            : base(context, trigger, triggerStatus, state, order)
        {
        }
    }
}