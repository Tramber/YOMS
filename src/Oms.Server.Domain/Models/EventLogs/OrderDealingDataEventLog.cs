using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Orders;

namespace Oms.Server.Domain.Models.EventLogs
{
    public class OrderDealingDataEventLog : DataEventLog<OrderStateMachine.Trigger, IOrderDealingData> 
    {
        public OrderDealingDataEventLog()
        {
            
        }
        public OrderDealingDataEventLog(ITriggerContext context, OrderStateMachine.Trigger trigger, TriggerStatus status, IOrderDealingData dealingData)
        : base(context, trigger, status, dealingData)
        {
        }
    }
}