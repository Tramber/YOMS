using System;
using System.Collections.Generic;
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
        private ITrade _incomingTrade;
        private readonly OrderStateMachine _stateMachine;
        private const double Epsilon = 0.000001;
        private static readonly OrderValidator OrderValidator = new OrderValidator();

        public Order()
        {
            _stateMachine = CreateStateMachine();
        }

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

        /// <summary>
        /// A state machine whose guards choose between the targets of a trigger, from the data being worked on
        /// (<c>_workingData</c>, which already counts the incoming trade): fully executed orders go to Validating,
        /// the others stay in or go back to Dealing.
        /// </summary>
        private OrderStateMachine CreateStateMachine()
        {
            var stateMachine = OrderStateMachine.CreateInitializable();
            OrderStateMachine.GuardClauseDelegate executed = () => IsFullyExecuted(_workingData);
            OrderStateMachine.GuardClauseDelegate notExecuted = () => !IsFullyExecuted(_workingData);

            stateMachine.GuardClauseFromWorkingToDealingUsingTriggerAddTrade = notExecuted;
            stateMachine.GuardClauseFromWorkingToValidatingUsingTriggerAddTrade = executed;
            stateMachine.GuardClauseFromDealingToDealingUsingTriggerAddTrade = notExecuted;
            stateMachine.GuardClauseFromDealingToValidatingUsingTriggerAddTrade = executed;

            stateMachine.GuardClauseFromValidatingToDealingUsingTriggerCancelTrade = notExecuted;
            stateMachine.GuardClauseFromValidatingToValidatingUsingTriggerCancelTrade = executed;
            stateMachine.GuardClauseFromBookingToDealingUsingTriggerCancelTrade = notExecuted;
            stateMachine.GuardClauseFromBookingToBookingUsingTriggerCancelTrade = executed;

            // Booking ends with the last trade to book.
            stateMachine.GuardClauseFromBookingToBookingUsingTriggerTradeBooked = () => !AllTradesBooked();
            stateMachine.GuardClauseFromBookingToTerminatedUsingTriggerTradeBooked = AllTradesBooked;
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
            result.SetExecutionQuantity(ComputeExecutionQuantity(result.Side));
            return result.SetOrderStatus(ComputeOrderStatus(result));
        }

        /// <summary>The distinct, not cancelled trades of the dealing events, plus the incoming one.</summary>
        private IEnumerable<ITrade> ActiveTrades(ITrade incomingTrade = null)
        {
            return EventLogs.Where(e => e.TriggerStatus.IsValid())
                .OfType<OrderParameterEventLog<IOrderDealingData>>()
                .Select(e => e.Parameters.Trade)
                .Concat([incomingTrade])
                .Where(t => t != null && t.TradeState != TradeState.Cancelled)
                .Distinct();
        }

        /// <summary>Every trade of the order, cancelled ones included.</summary>
        public IList<ITrade> GetTrades()
        {
            return
            [
                .. EventLogs.Where(e => e.TriggerStatus.IsValid())
                    .OfType<OrderParameterEventLog<IOrderDealingData>>()
                    .Select(e => e.Parameters.Trade)
                    .Where(t => t != null)
                    .Distinct()
            ];
        }

        /// <summary>Quantity executed so far; opposite-side trades count against it.</summary>
        private double ComputeExecutionQuantity(Side side, ITrade incomingTrade = null)
        {
            return ActiveTrades(incomingTrade).Sum(t => t.Side == side ? t.ExecutionQuantity : -t.ExecutionQuantity);
        }

        /// <summary>True when the order has trades and every one of them (counting the incoming one) is booked.</summary>
        private bool AllTradesBooked()
        {
            var trades = ActiveTrades(_incomingTrade).ToList();
            return trades.Count > 0 && trades.All(t => t.TradeState == TradeState.Booked);
        }

        private OrderStatus ComputeOrderStatus(OrderData orderData)
        {
            // A pending Cancel or Update shows in whatever live state the order is.
            if (orderData.OrderState != OrderStateMachine.State.Terminated && orderData.OrderState != OrderStateMachine.State.Undefined)
            {
                if (orderData.PendingTrigger == OrderStateMachine.Trigger.Cancel) return OrderStatus.CancelPending;
                if (orderData.PendingTrigger == OrderStateMachine.Trigger.Update) return OrderStatus.UpdatePending;
            }
            switch (orderData.OrderState)
            {
                case OrderStateMachine.State.Terminated:
                    // Terminated by the last booking: the order was executed, not cancelled.
                    if (orderData.Trigger == OrderStateMachine.Trigger.TradeBooked)
                        return orderData.RemainingQuantity < -Epsilon ? OrderStatus.OverFilled : OrderStatus.Filled;
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
                    // Partially filled: something was executed and something remains.
                    if (orderData.RemainingQuantity > Epsilon && orderData.Quantity - orderData.RemainingQuantity > Epsilon)
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

        /// <summary>Everything ordered is executed, or more (an over-fill still leaves Dealing).</summary>
        private bool IsFullyExecuted(OrderData orderData)
        {
            return orderData.Quantity > Epsilon && orderData.RemainingQuantity <= Epsilon;
        }

        private bool IsFilled(OrderData orderData)
        {
            // Filled: something was ordered and nothing remains to execute.
            return orderData.Quantity > Epsilon && Math.Abs(orderData.RemainingQuantity) <= Epsilon;
        }
    }
}
