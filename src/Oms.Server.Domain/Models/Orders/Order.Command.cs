using System;
using System.Linq;
using System.Diagnostics.Contracts;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Validation;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.Orders
{
    partial class Order
    {
        private OrderData _workingData;

        /// <summary>Validates the core and editable data and saves the order as a draft (Undefined to Draft).</summary>
        public GenericResult Create(ITriggerContext context, IOrderCoreData coreData, IOrderEditableData editableData)
        {
            return HandleEditionCommand(OrderStateMachine.Trigger.Create, context, editableData, coreData);
        }

        /// <summary>Validates the data and sends the order for acceptance (Undefined or Draft to Accepting).</summary>
        public GenericResult SendRequest(ITriggerContext context, IOrderCoreData coreData, IOrderEditableData editableData)
        {
            return HandleEditionCommand(OrderStateMachine.Trigger.SendRequest, context, editableData, coreData);
        }

        /// <summary>Edits the order data. Update is a pending command: it is logged as Pending and only applied once <see cref="AcceptPending"/> is called.</summary>
        public GenericResult Update(ITriggerContext context, IOrderCoreData coreData, IOrderEditableData editableData)
        {
            return HandleEditionCommand(OrderStateMachine.Trigger.Update, context, editableData, coreData);
        }

        #region Simple Order State actions

        /// <summary>Requests the cancellation of the order. Cancel is a pending command: the order moves to Terminated only once <see cref="AcceptPending"/> is called.</summary>
        public GenericResult Cancel(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.Cancel, context);
        }

        /// <summary>Validates the core and editable data, then runs an editing trigger (Create, SendRequest or Update) with them.</summary>
        public GenericResult HandleEditionCommand(OrderStateMachine.Trigger trigger,
            ITriggerContext context,
            IOrderEditableData editableData = null,
            IOrderCoreData coreData = null,
            TriggerStatus status = TriggerStatus.Done)
        {
            var resultCore = OrderValidator.OrderCoreValidator.IsValid(coreData);
            if (resultCore.IsSuccess())
            {
                var resultEditable = OrderValidator.OrderEditableValidator.IsValid(editableData);
                return resultEditable.IsSuccess() ? HandleCommand(trigger, context, editableData, null, coreData, status) : resultEditable;
             }
            return resultCore;
        }

        /// <summary>Accepts the pending Cancel or Update and applies it. Fails when nothing is pending.</summary>
        public GenericResult AcceptPending(ITriggerContext context, IOrderEditableData editableData = null, IOrderDealingData dealingData = null)
        {
            return HandlePendingReplyCommand(true, context, editableData, dealingData);
        }

        /// <summary>Rejects the pending Cancel or Update; the order keeps its state. Fails when nothing is pending.</summary>
        public GenericResult RejectPending(ITriggerContext context, IOrderEditableData editableData = null, IOrderDealingData dealingData = null)
        {
            return HandlePendingReplyCommand(false, context, editableData, dealingData);
        }

        /// <summary>Deletes a draft (Draft to Terminated).</summary>
        public GenericResult Delete(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.Delete, context);
        }
        /// <summary>Takes the order back from acceptance to edit it again (Accepting to Draft).</summary>
        public GenericResult Recall(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.Recall, context);
        }

        /// <summary>Starts the booking of a fully executed order (Validating to Booking).</summary>
        public GenericResult StartBooking(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.StartBooking, context);
        }

        /// <summary>Sends the order for acceptance without changing its data (Undefined or Draft to Accepting).</summary>
        public GenericResult SendRequest(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.SendRequest, context);
        }

        internal GenericResult SendRequest(TriggerContext context, IOrderCoreData coreData, IOrderEditableData editableData)
        {
            return HandleCommand(OrderStateMachine.Trigger.SendRequest, context, editableData, null, coreData);
        }

        /// <summary>Records that the order was refused (Accepting to Terminated).</summary>
        public GenericResult SendReject(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.SendReject, context);
        }
        /// <summary>Records that the order was accepted (Accepting to Working).</summary>
        public GenericResult SendAccept(ITriggerContext context)
        {
            return HandleCommand(OrderStateMachine.Trigger.SendAccept, context);
        }
        #endregion

        #region Market send actions

        /// <summary>Sends the order to the market (Working or Dealing to Dealing). The dealing data describes the route.</summary>
        public GenericResult MarketSend(ITriggerContext context, IOrderDealingData dealingData)
        {
            return HandleCommand(OrderStateMachine.Trigger.MarketSend, context, dealingData: dealingData);
        }

        /// <summary>Cancels the order at the market while it stays in Dealing.</summary>
        public GenericResult MarketCancel(ITriggerContext context, IOrderDealingData dealingData)
        {
            return HandleCommand(OrderStateMachine.Trigger.MarketCancel, context, dealingData: dealingData);
        }

        #endregion

        #region Trade actions
        /// <summary>
        /// Records that a trade was booked (the trade moves from BookingSent to Booked unless it is already Booked).
        /// The order stays in Booking while trades remain to book, and terminates once every trade is booked.
        /// </summary>
        public GenericResult TradeBooked(ITriggerContext context, ITrade trade)
        {
            return HandleTradeCommand(OrderStateMachine.Trigger.TradeBooked, context, trade);
        }

        /// <summary>Records an execution on the order. Cannot succeed yet: the Dealing/Validating transitions have no guard clauses wired.</summary>
        public GenericResult AddTrade(ITriggerContext context, ITrade trade)
        {
            return HandleTradeCommand(OrderStateMachine.Trigger.AddTrade, context, trade);
        }

        /// <summary>Cancels an execution of the order. Works from Dealing; from Validating and Booking the state machine still lacks guards.</summary>
        public GenericResult CancelTrade(ITriggerContext context, ITrade trade)
        {
            return HandleTradeCommand(OrderStateMachine.Trigger.CancelTrade, context, trade);
        }
        #endregion

        private static bool IsTriggerHandlePending(OrderStateMachine.Trigger trigger)
        {
            //TODO handle if the context.user has a bypass
            return trigger == OrderStateMachine.Trigger.Cancel || trigger == OrderStateMachine.Trigger.Update;
        }

        private GenericResult HandlePendingReplyCommand(
            bool isAccept,
            ITriggerContext context,
            IOrderEditableData editableData = null,
            IOrderDealingData dealingData = null,
            IOrderCoreData coreData = null)
        {
            if (PendingTrigger == null)
                return GenericResult.FailureFormat("There is no pending command to {0}", isAccept ? "accept" : "reject");
            // Accepting an Update applies the data that was sent with it.
            if (isAccept && editableData == null && PendingTrigger == OrderStateMachine.Trigger.Update)
                editableData = PendingEditableData();
            return HandleCommand(PendingTrigger.Value, context, editableData, dealingData, coreData, isAccept ? TriggerStatus.Accepted : TriggerStatus.Rejected);
        }

        /// <summary>The data sent with the Update that is still waiting for an answer.</summary>
        private IOrderEditableData PendingEditableData()
        {
            var pending = EventLogs.OfType<OrderParameterEventLog<IOrderEditableData>>()
                .LastOrDefault(e => e.Trigger == OrderStateMachine.Trigger.Update && e.TriggerStatus.IsPending());
            return pending == null ? null : pending.Parameters;
        }

        private GenericResult EnforceTriggerStatus(OrderStateMachine.Trigger trigger, ref TriggerStatus triggerStatus)
        {
            if (IsTriggerHandlePending(trigger))
            {
                if (_workingData.PendingTrigger != null)
                {
                    if(!triggerStatus.IsPendingReply())
                        return GenericResult.FailureFormat("There is already a {0} pending command. You must handle it before", _workingData.PendingTrigger);
                    if(trigger != _workingData.PendingTrigger)
                        return GenericResult.FailureFormat("There is a {0} pending not a {1}. You cannot {0} it", _workingData.PendingTrigger, trigger, triggerStatus);
                    _workingData.SetPendingTrigger(null);
                }
                else
                {
                    triggerStatus = TriggerStatus.Pending;
                    _workingData.SetPendingTrigger(trigger);
                }
            }
            else if (triggerStatus.IsPending() || triggerStatus.IsPendingReply())
            {
                return GenericResult.FailureFormat("The command {0} could not be {1}. No pending required", trigger, triggerStatus); 
            }
            return GenericResult.Success();
        }


        private GenericResult HandleCommand(
            OrderStateMachine.Trigger trigger,
            ITriggerContext context,
            IOrderEditableData editableData = null,
            IOrderDealingData dealingData = null,
            IOrderCoreData coreData = null,
            TriggerStatus status = TriggerStatus.Done)
        {
            try
            {
                return RunCommand(trigger, context, editableData, dealingData, coreData, status);
            }
            finally
            {
                _workingData = null;
                _incomingTrade = null;
            }
        }

        private GenericResult RunCommand(
            OrderStateMachine.Trigger trigger,
            ITriggerContext context,
            IOrderEditableData editableData,
            IOrderDealingData dealingData,
            IOrderCoreData coreData,
            TriggerStatus status)
        {
            Contract.Requires(context != null, "context != null" );

            _workingData = CurrentData.Clone()
                .SetRoutingData(dealingData)
                .SetTrigger(trigger)
                .SetPendingTrigger(this.PendingTrigger);
            // The guards of the state machine decide on what remains and what is booked once this trade is counted.
            _incomingTrade = dealingData == null ? null : dealingData.Trade;
            if (_incomingTrade != null)
                _workingData.SetExecutionQuantity(ComputeExecutionQuantity(_workingData.Side, _incomingTrade));
            var result = EnforceTriggerStatus(trigger, ref status);
            if (result.IsFailure())
            {
                return result;
            }
            // The data of a command that waits for an answer, or was refused, is not applied.
            if (status != TriggerStatus.Pending && status != TriggerStatus.Rejected)
                _workingData.SetEditableData(editableData);
            if (status == TriggerStatus.Rejected)
            {
                result = GenericResult.Success();
            }
            else
            {
                var triggerSucceeded = status == TriggerStatus.Pending ? _stateMachine.CanFireTrigger(trigger) : _stateMachine.TryFireTrigger(trigger);
                result = triggerSucceeded ? GenericResult.Success() : GenericResult.Failure(string.Concat(_workingData.StateMachineErrorMessage, String.Format("The commmand {0} is not allowed when the order is in {1} state", trigger, OrderState)));
            }
            if (result.IsSuccess())
            {
                _workingData.SetOrderState(_stateMachine.GetState);
                if (editableData != null)
                {
                    // Core data of a command that waits for an answer is not applied either.
                    if (coreData != null && status.IsValid()) this.InsertDataFrom(coreData);
                    EventLogs.Add(new OrderParameterEventLog<IOrderEditableData>(context, trigger, status, _workingData.OrderState, this, editableData));
                }
                else if (dealingData != null)
                {
                    EventLogs.Add(new OrderParameterEventLog<IOrderDealingData>(context, trigger, status, _workingData.OrderState, this, dealingData));
                }
                else
                    EventLogs.Add(new OrderEventLog(context, trigger, status, _workingData.OrderState, this));
                // After the event is logged: new trades, and a new quantity from an update, both change what remains.
                _workingData.SetExecutionQuantity(ComputeExecutionQuantity(_workingData.Side));
                _workingData.SetOrderStatus(ComputeOrderStatus(_workingData));
                RefreshCurrentData(_workingData);
            }
            _workingData = null;
            return result;
        }

        private GenericResult HandleTradeCommand(
            OrderStateMachine.Trigger trigger,
            ITriggerContext context,
            ITrade trade,
            ITradeEditableData editableData = null
            )
        {
            Contract.Requires(context != null, "context != null");

            _workingData = CurrentData.Clone();
            if (!_stateMachine.CanFireTrigger(trigger))
            {
                return GenericResult.FailureFormat(String.Format("The commmand {0} is not allowed when the _order is in {1} state", trigger, OrderState));
            }
            GenericResult result;
            switch (trigger)
            {
                case OrderStateMachine.Trigger.AddTrade:
                    // The trade passed in carries the data of the execution; the order owns a trade of its own.
                    var newTrade = new Trade {Order = this};
                    result = newTrade.Create(context, editableData ?? trade);
                    trade = newTrade;
                    break;
                case OrderStateMachine.Trigger.TradeBooked:
                    if (trade == null)
                        return GenericResult.Failure("The trade is required");
                    // A trade forced to booked by hand is already booked.
                    result = trade.TradeState == TradeState.Booked ? GenericResult.Success() : trade.Booked(context);
                    break;
                case OrderStateMachine.Trigger.CancelTrade:
                    if (trade == null)
                        return GenericResult.Failure("The trade is required");
                    result = trade.Cancel(context);
                    break;
                case OrderStateMachine.Trigger.UpdateTrade:
                default:
                    return GenericResult.FailureFormat("The trade command {0} is not implemented", trigger);
            }
            return result.IsSuccess() ? HandleCommand(trigger, context, dealingData: new OrderDealingEventParameter(trade)) : result;
        }
    }
}
