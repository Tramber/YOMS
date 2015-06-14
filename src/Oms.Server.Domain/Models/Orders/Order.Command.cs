using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Generics;
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



        public GenericResult Cancel(ITriggerContext context)
        {
            //TODO handle the pending
            return HandleCommand(OrderStateMachine.Trigger.Cancel, context);
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

        public GenericResult AddTrade(ITriggerContext context, int tradeId)
        {
            //TODO Recompute remaining
            return HandleCommand(OrderStateMachine.Trigger.AddTrade, context);
        }

        public GenericResult CancelTrade(ITriggerContext context, int tradeId)
        {
            //TODO Recompute remaining
            return HandleCommand(OrderStateMachine.Trigger.CancelTrade, context);
        }

        public GenericResult MarketSend(ITriggerContext context, IOrderDealingData dealingData)
        {
            return HandleCommand(OrderStateMachine.Trigger.MarketSend, context);
        }

        public GenericResult MarketCancel(ITriggerContext context, IOrderDealingData dealingData)
        {
            return HandleCommand(OrderStateMachine.Trigger.MarketCancel, context);
        }

        public GenericResult TradeBooked(ITriggerContext context, int tradeId)
        {
            return HandleCommand(OrderStateMachine.Trigger.TradeBooked, context);
        }

        public GenericResult ResumeSnapshot(IOrderCoreData coreData, IOrderTransientData transientData, double? remainingQuantity)
        {

            return GenericResult.Failure("Not implemented");
        }

        private GenericResult HandleCommand(
            OrderStateMachine.Trigger trigger, 
            ITriggerContext context,
            IOrderTransientData transientData = null,
            IOrderDealingData dealingData = null,
            IOrderCoreData coreData = null)
        {
            workingData = CurrentData.Clone().SetDealingData(dealingData).SetTransientData(transientData);
            var hasSucceeded = StateMachine.TryFireTrigger(OrderStateMachine.Trigger.Create);
            var result = hasSucceeded ? GenericResult.Success() : GenericResult.Failure(workingData.StateMachineErrorMessage);
            if (hasSucceeded)
            {
                if (transientData != null)
                {
                    if (coreData != null) this.InsertDataFrom(coreData);
                    EventLogs.Add(new OrderDataEventLog(context, trigger, TriggerStatus.Accepted, transientData));
                }
                else if (dealingData != null)
                    EventLogs.Add(new OrderDealingDataEventLog(context, trigger, TriggerStatus.Accepted, dealingData));
                else
                    EventLogs.Add(new OrderStateEventLog(context, trigger, TriggerStatus.Accepted));
            }
            return result;
        }
    }
}
