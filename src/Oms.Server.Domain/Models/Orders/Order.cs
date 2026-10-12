using System;
using System.Linq;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Validation;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.Orders
{
    public partial class Order : IOrder
    {
        private OrderData _currentDataDoNotUseDirectly;
        private readonly OrderStateMachine _stateMachine = OrderStateMachine.CreateInitializable();
        private const double Epsilon = 0.000001;
        private static readonly OrderValidator OrderValidator = new OrderValidator();

        private OrderData CurrentData
        {
            get
            {
                if (_currentDataDoNotUseDirectly == null)
                    RefreshCurrentData();
                return _currentDataDoNotUseDirectly;
            }
        }

        private void RefreshCurrentData(OrderData orderData = null)
        {
            _currentDataDoNotUseDirectly = orderData ?? ComputeOrderData();
            _stateMachine.Initialize(_currentDataDoNotUseDirectly.OrderState);
        }

        private OrderStateMachine CreateStateMachine()
        {
            var stateMachine = OrderStateMachine.CreateInitializable();
            stateMachine.GuardClauseFromDealingToValidatingUsingTriggerAddTrade = () => IsFilled(_workingData);
            stateMachine.GuardClauseFromWorkingToValidatingUsingTriggerAddTrade = () => IsFilled(_workingData);
            stateMachine.GuardClauseFromUndefinedToAcceptingUsingTriggerSendRequest = () => IsFilled(_workingData);
            return stateMachine;
        }

        private OrderData ComputeOrderData()
        {
            var orderedEventLogs = EventLogs.Where(e => !e.TriggerStatus.IsFailure()).OrderBy(e => e.Context.Date).ThenBy(e => e.Id).ToList();

            var lastEditableEvent = orderedEventLogs.OfType<OrderParameterEventLog<IOrderEditableData>>().LastOrDefault(e => e.TriggerStatus.IsValid());
            var lastDealEvent = orderedEventLogs.OfType<OrderParameterEventLog<IOrderDealingData>>().LastOrDefault(e => e.TriggerStatus.IsValid());
            var lastValidEvent = orderedEventLogs.LastOrDefault();
            var lastPendingEvent = orderedEventLogs.LastOrDefault(e => e.TriggerStatus.IsPending());
            // A pending trigger that was accepted or rejected since is no longer pending.
            if (lastPendingEvent != null
                && orderedEventLogs.Any(e => e.Trigger == lastPendingEvent.Trigger && e.Context.Date >= lastPendingEvent.Context.Date && e.TriggerStatus.IsPendingReply()))
            {
                lastPendingEvent = null;
            }
            if (lastValidEvent == null)
            {
                return new OrderData();
            }
            var result = lastEditableEvent == null ? new OrderData() : new OrderData()
                .SetEditableData(lastEditableEvent.Parameters)
                .SetRoutingData(lastDealEvent?.Parameters)
                .SetOrderState(lastValidEvent.State)
                .SetTrigger(lastValidEvent.Trigger)
                .SetPendingTrigger(lastPendingEvent?.Trigger);
            return result.SetOrderStatus(ComputeOrderStatus(result));
        }

        private double ComputeExecutionQuantity(Side side)
        {
            return EventLogs.OfType<OrderParameterEventLog<OrderDealingEventParameter>>()
                .Select(e => e.Parameters.Trade)
                .Where(t => t.TradeState != TradeState.Cancelled)
                .Sum(t => t.Side == side ? t.ExecutionQuantity : -t.ExecutionQuantity);
        }

        private OrderStatus ComputeOrderStatus(OrderData orderData)
        {
            switch (orderData.OrderState)
            {
                case OrderStateMachine.State.Terminated:
                    return IsFilled(orderData) ? OrderStatus.CancelledFilled : OrderStatus.Cancelled;
                case OrderStateMachine.State.Booking:
                case OrderStateMachine.State.Validating:
                    if (orderData.RemainingQuantity < -Epsilon)
                        return OrderStatus.OverFilled;
                    if (IsFilled(orderData))
                        return OrderStatus.Filled;
                    goto case OrderStateMachine.State.Dealing;
                case OrderStateMachine.State.Dealing:
                case OrderStateMachine.State.Working:
                    if (orderData.PendingTrigger == OrderStateMachine.Trigger.Cancel) return OrderStatus.CancelPending;
                    if (orderData.PendingTrigger == OrderStateMachine.Trigger.Update) return OrderStatus.UpdatePending;
                    if (orderData.RemainingQuantity > Epsilon)
                        return OrderStatus.PartiallyFilled;
                    goto case OrderStateMachine.State.Accepting;
                case OrderStateMachine.State.Accepting:
                    return OrderStatus.New;
                case OrderStateMachine.State.Draft:
                    return OrderStatus.Pending;
                default:
                    return OrderStatus.Undefined;
            }
        }

        private bool IsFilled(OrderData orderData)
        {
            return Math.Abs(orderData.Quantity - orderData.RemainingQuantity) <= Epsilon;
        }
    }
}
