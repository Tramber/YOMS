using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class OrderDealingDataEventLog : DataEventLog<OrderStateMachine.Trigger, IOrderDealingData> , IIdentifiable
    {
        public OrderDealingDataEventLog()
        {
            
        }
        public OrderDealingDataEventLog(ITriggerContext context, OrderStateMachine.Trigger trigger, TriggerStatus status, IOrderDealingData dealingData)
        : base(context, trigger, status, dealingData)
        {
        }

        public int Id { get; set; }
    }
}