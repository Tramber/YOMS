using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Interfaces.Models
{
    public interface IOrderComputedData
    {
        OrderStateMachine.State OrderState { get; }

        OrderStatus OrderStatus { get; }

        double RemainingQuantity { get; }

        OrderStateMachine.Trigger? PendingTrigger { get; }
    }
}