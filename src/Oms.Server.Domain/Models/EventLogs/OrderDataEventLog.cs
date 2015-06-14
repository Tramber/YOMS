using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class OrderDataEventLog : DataEventLog<OrderStateMachine.Trigger, IOrderTransientData>
    {
        public OrderDataEventLog()
        {
        }

        public OrderDataEventLog(ITriggerContext context, OrderStateMachine.Trigger trigger, TriggerStatus status, IOrderTransientData data) : base(context, trigger, status, data) { }
    }
}