using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.Orders
{
    partial class Order
    {
        private OrderData workingData;

        public GenericResult Create(ITriggerContext context, IOrderCoreData coreData, IOrderTransientData transientData)
        {
            return HandleCommand(OrderStateMachine.Trigger.Create, context, transientData, null, coreData);
        }

        public GenericResult Cancel(ITriggerContext context, bool? isAccepted = null)
        {
            //TODO (BL) most of the behavior is generic here, it could be moved elsewhere
            const OrderStateMachine.Trigger trigger = OrderStateMachine.Trigger.Cancel;
            if (isAccepted == null)
            {
                if(this.PendingTrigger != null)
                    return GenericResult.FailureFormat("There is already a {0} action pending. You could not ask for {1}", this.PendingTrigger, trigger);
                return HandleCommand(trigger, context, null, null, null, TriggerStatus.Pending);
            }
            var status = isAccepted.Value ? TriggerStatus.Accepted : TriggerStatus.Rejected;
            if (PendingTrigger == null)
                return GenericResult.FailureFormat("There is no pending action, it cannot be {0}", status);
            if (PendingTrigger.Value != OrderStateMachine.Trigger.Cancel)
                return GenericResult.FailureFormat("There is no pending {0} action, it cannot be {0}", trigger, status);
            return HandleCommand(trigger, context, null, null, null, status);
        }

        public GenericResult Delete(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.Delete, context);
        }
        public GenericResult Recall(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.Recall, context);
        }

        public GenericResult StartBooking(ITriggerContext context)
        {
            //TODO DomainEvent SendToBooking
            return HandleCommand(OrderStateMachine.Trigger.StartBooking, context);
        }

        public GenericResult SendRequest(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.SendRequest, context);
        }

        internal GenericResult SendRequest(TriggerContext context, OrderCoreData coreData, OrderTransientData transientData)
        {
            return HandleCommand(OrderStateMachine.Trigger.SendRequest, context, transientData, null, coreData);
        }

        public GenericResult SendReject(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.SendReject, context);
        }
        public GenericResult SendAccept(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.SendAccept, context);
        }

        public GenericResult AddTrade(ITriggerContext context, Trade trade)
        {
            return UpdateTrade(context, trade, OrderStateMachine.Trigger.AddTrade);
        }

        public GenericResult CancelTrade(ITriggerContext context, Trade trade)
        {
            return UpdateTrade(context, trade, OrderStateMachine.Trigger.Delete);
        }

        private GenericResult UpdateTrade(ITriggerContext context, Trade trade, OrderStateMachine.Trigger tradeTrigger)
        {
            if (!StateMachine.CanFireTrigger(tradeTrigger))
            {
                return GenericResult.FailureFormat("It's not possible to cancel the Trade {0} while the order is in state {1} ({2})", trade.Id, OrderState, OrderStatus);
            }
            var tradeResult = tradeTrigger.ForwardCommand(trade);
            if (tradeResult.IsFailure()) return tradeResult;
            return HandleCommand(OrderStateMachine.Trigger.CancelTrade, context, null, new OrderDealingData(trade));
        }

        public GenericResult MarketSend(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.MarketSend, context);
        }

        public GenericResult MarketCancel(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.MarketCancel, context);
        }

        public GenericResult TradeBooked(ITriggerContext context, Trade trade)
        {
            return HandleCommand(OrderStateMachine.Trigger.TradeBooked, context, null, new OrderDealingData(trade));
        }

        public GenericResult ResumeSnapshot(OrderStateMachine.State orderState, IOrderCoreData coreData, IOrderTransientData transientData)
        {
            this.InsertDataFrom(coreData);
            EventLogs.Add(new OrderDataEventLog(new TriggerContext(), OrderStateMachine.Trigger.ResumeSnapshot, TriggerStatus.Accepted, transientData));
            var snapshotData = this.CurrentData.Clone().SetTransientData(transientData);
            snapshotData.OrderState = orderState;
            snapshotData.OrderStatus = ComputeOrderStatus(snapshotData);
            RefreshCurrentData(snapshotData);
            return GenericResult.Success();
        }

        private GenericResult HandleCommand(
            OrderStateMachine.Trigger trigger, 
            ITriggerContext context,
            IOrderTransientData transientData = null,
            IOrderDealingData dealingData = null,
            IOrderCoreData coreData = null,
            TriggerStatus status = TriggerStatus.Accepted)
        {
            workingData = CurrentData.Clone().SetRoutingData(dealingData).SetTransientData(transientData);
            var hasSucceeded = status == TriggerStatus.Pending ? StateMachine.CanFireTrigger(trigger) : StateMachine.TryFireTrigger(trigger);
            var result = hasSucceeded ? GenericResult.Success() : GenericResult.Failure(string.Concat(workingData.StateMachineErrorMessage, String.Format("Workflow do not allow you to {0} from {1}", trigger, OrderState)));
            if (hasSucceeded)
            {
                if (transientData != null)
                {
                    if (coreData != null) this.InsertDataFrom(coreData);
                    EventLogs.Add(new OrderDataEventLog(context, trigger, status, transientData));
                }
                else if (dealingData != null)
                {
                    EventLogs.Add(new OrderDealingDataEventLog(context, trigger, status, dealingData));
                    if (dealingData.Trade != null)
                        workingData.RemainingQuantity = ComputeRemainingQuantity();
                }
                else
                    EventLogs.Add(new OrderStateEventLog(context, trigger, status));
                workingData.OrderStatus = ComputeOrderStatus(workingData);
                if (status == TriggerStatus.Pending)
                {
                    workingData.PendingTrigger = trigger;
                }
                else if(workingData.PendingTrigger != null && workingData.PendingTrigger.Value == trigger)
                {
                    workingData.PendingTrigger = null;
                }
                RefreshCurrentData(workingData);
                workingData = null;
            }
            return result;
        }
    }
}
