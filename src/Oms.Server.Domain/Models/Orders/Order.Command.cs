using System;
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
        /// <summary>Records that a trade was booked. Not finished: the booking outcome is still a TODO.</summary>
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
            return HandleCommand(PendingTrigger.Value, context, editableData, dealingData, coreData, isAccept ? TriggerStatus.Accepted : TriggerStatus.Rejected);
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
            Contract.Requires(context != null, "context != null" );

            _workingData = CurrentData.Clone()
                .SetRoutingData(dealingData)
                .SetEditableData(editableData)
                .SetTrigger(trigger)
                .SetPendingTrigger(this.PendingTrigger);
            var result = EnforceTriggerStatus(trigger, ref status);
            if (result.IsFailure())
            {
                return result;
            }
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
                    if (coreData != null) this.InsertDataFrom(coreData);
                    EventLogs.Add(new OrderParameterEventLog<IOrderEditableData>(context, trigger, status, _workingData.OrderState, this, editableData));
                }
                else if (dealingData != null)
                {
                    EventLogs.Add(new OrderParameterEventLog<IOrderDealingData>(context, trigger, status, _workingData.OrderState, this, dealingData));
                    if (dealingData.Trade != null)
                        _workingData.SetExecutionQuantity(ComputeExecutionQuantity(_workingData.Side));
                }
                else
                    EventLogs.Add(new OrderEventLog(context, trigger, status, _workingData.OrderState, this));
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
                    trade = new Trade {Order = this};
                    result = trade.Create(context, editableData);
                    break;
                case OrderStateMachine.Trigger.TradeBooked:
                    //TODO handle the booking done
                    result = GenericResult.Success();
                    break;
                case OrderStateMachine.Trigger.CancelTrade:
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
