using Oms.Server.Domain.Models.Orders;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class OrderStateEventLog : EventLog<OrderStateMachine.Trigger>
    {
        public OrderStateEventLog()
        {
        }

        public OrderStateEventLog(Interfaces.Models.ITriggerContext context, OrderStateMachine.Trigger trigger, TriggerStatus status)
            : base(context, trigger, status)
        {
        }
    }
}