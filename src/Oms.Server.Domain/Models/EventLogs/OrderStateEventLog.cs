using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class OrderStateEventLog : EventLog<OrderStateMachine.Trigger>
    {
        public OrderStateEventLog()
        {
        }

        public OrderStateEventLog(Interfaces.Models.ITriggerContext context, OrderStateMachine.Trigger trigger, TriggerStatus status, int? relatedEventLogId = null)
            : base(context, trigger, status)
        {
        }
    }
}