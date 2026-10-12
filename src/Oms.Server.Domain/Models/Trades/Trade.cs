using System;
using System.Collections.Generic;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;

namespace Oms.Server.Domain.Models.Trades
{
    /// <summary>
    /// An execution on an order. Every change is recorded as a <see cref="TradeEventLog"/>.
    /// Life cycle: New, then BookingSent once sent to the booking system, then Booked (or BookingError, which can be
    /// sent again). A trade can be forced to Booked by hand from New or BookingError, and cancelled unless it is
    /// already cancelled. The Booking state is the order's, a trade never goes through it.
    /// </summary>
    public class Trade : ITrade
    {
        public int Id { get; set; }

        public IList<TradeEventLog> EventLogs { get; set; } = new List<TradeEventLog>();

        public double ExecutionQuantity { get; set; }

        public double ExecutionAmountNet { get; set; }

        public TradeState TradeState { get; set; }

        public Side Side { get; set; }

        public IOrder Order { get; set; }

        public GenericResult Create(ITriggerContext context, ITradeEditableData editableData)
        {
            if (TradeState != TradeState.Undefined)
                return GenericResult.FailureFormat("The trade is already created ({0})", TradeState);
            if (editableData == null)
                return GenericResult.Failure("The trade data are required");
            if (editableData.ExecutionQuantity <= 0)
                return GenericResult.Failure("The execution quantity must be positive");

            ExecutionQuantity = editableData.ExecutionQuantity;
            ExecutionAmountNet = editableData.ExecutionAmountNet;
            Side = editableData.Side;
            TradeState = TradeState.New;
            // A snapshot of the data, so that what is logged is always the persisted parameter type.
            var snapshot = new TradeEditableEventParameter
            {
                ExecutionQuantity = ExecutionQuantity, ExecutionAmountNet = ExecutionAmountNet, Side = Side, TradeState = TradeState,
            };
            EventLogs.Add(new TradeParameterEventLog<ITradeEditableData>(
                context, TradeTrigger.Create, TriggerStatus.Done, TradeState, this, snapshot));
            return GenericResult.Success();
        }

        public GenericResult Cancel(ITriggerContext context)
        {
            if (TradeState == TradeState.Undefined)
                return GenericResult.Failure("The trade is not created yet");
            if (TradeState == TradeState.Cancelled)
                return GenericResult.Failure("The trade is already cancelled");

            return Move(context, TradeTrigger.Cancel, TradeState.Cancelled,
                TradeState.New, TradeState.BookingSent, TradeState.Booked, TradeState.BookingError);
        }

        /// <summary>Sends the trade to the booking system (New or BookingError to BookingSent).</summary>
        public GenericResult BookingSend(ITriggerContext context)
        {
            return Move(context, TradeTrigger.BookingSend, TradeState.BookingSent, TradeState.New, TradeState.BookingError);
        }

        /// <summary>The booking system confirmed the trade (BookingSent to Booked).</summary>
        public GenericResult Booked(ITriggerContext context)
        {
            return Move(context, TradeTrigger.Booked, TradeState.Booked, TradeState.BookingSent);
        }

        /// <summary>Marks the trade as booked by hand, without the booking system (New or BookingError to Booked).</summary>
        public GenericResult ForceBooked(ITriggerContext context)
        {
            return Move(context, TradeTrigger.ForceBooked, TradeState.Booked, TradeState.New, TradeState.BookingError);
        }

        /// <summary>Asks the booking system again for the outcome of a trade it has not answered for (stays BookingSent).</summary>
        public GenericResult BookingRefresh(ITriggerContext context)
        {
            return Move(context, TradeTrigger.BookingRefresh, TradeState.BookingSent, TradeState.BookingSent);
        }

        /// <summary>The booking system refused the trade (BookingSent to BookingError).</summary>
        public GenericResult BookingFailed(ITriggerContext context)
        {
            return Move(context, TradeTrigger.BookingFailed, TradeState.BookingError, TradeState.BookingSent);
        }

        private GenericResult Move(ITriggerContext context, TradeTrigger trigger, TradeState to, params TradeState[] allowedFrom)
        {
            if (Array.IndexOf(allowedFrom, TradeState) < 0)
                return GenericResult.FailureFormat("The trade cannot {0} when it is {1}", trigger, TradeState);

            TradeState = to;
            EventLogs.Add(new TradeEventLog(context, trigger, TriggerStatus.Done, TradeState, this));
            return GenericResult.Success();
        }
    }
}
