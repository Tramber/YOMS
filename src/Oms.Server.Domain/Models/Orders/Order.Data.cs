using System;
using System.Collections.Generic;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.Orders
{
    /// <summary>
    /// An order. It has no stored state: every accepted command appends an <see cref="OrderEventLog"/> to <see cref="EventLogs"/>,
    /// and the values exposed here (state, quantity, status, ...) are computed from that log. Commands are in Order.Command.cs;
    /// each one fires a trigger on the <see cref="OrderStateMachine"/> and returns a <see cref="Oms.Server.Domain.Framework.GenericResult"/>.
    /// </summary>
    public partial class Order : IOrderEditableData, IOrderComputedData, IOrderRoutingData
    {
        public int Id { get; set; }
        /// <summary>Append-only history of the order: one entry per accepted command, including pending, accepted and rejected replies.</summary>
        public IList<OrderEventLog> EventLogs { get; set; } = new List<OrderEventLog>();
        public IUser Owner { get; internal set; }
        public IUser Creator { get; internal set; }
        public DateTime CreationDate { get; internal set; }
        public IOrderBasket OrderBasket { get; internal set; }
        public IOrderInitialReferentialData InitialReferentialData { get; internal set; }
        public string ClientOrderRef { get; internal set; }
        public bool IsAutoAccepted { get; internal set; }

        /// <summary>Current workflow state. It does not change while a Cancel or Update is pending.</summary>
        public OrderStateMachine.State OrderState
        {
            get { return CurrentData.OrderState; }
        }

        /// <summary>The Cancel or Update waiting for <see cref="Order.AcceptPending"/> or <see cref="Order.RejectPending"/>, or null when nothing is pending.</summary>
        public OrderStateMachine.Trigger? PendingTrigger
        {
            get { return CurrentData.PendingTrigger; }
        }

        public double Quantity
        {
            get { return CurrentData.Quantity; }
        }

        public double? PriceLimit
        {
            get { return CurrentData.PriceLimit; }
        }


        public double? PriceStop
        {
            get { return CurrentData.PriceStop; }
        }

        public DateTime? SettlementDate
        {
            get { return CurrentData.SettlementDate; }
        }

        public Side Side
        {
            get { return CurrentData.Side; }
        }

        public ISecurity Security
        {
            get { return CurrentData.Security; }
        }

        public OrderType OrderType
        {
            get { return CurrentData.OrderType; }
        }

        public OrderValidityType OrderValidity
        {
            get { return CurrentData.OrderValidity; }
        }

        public DateTime? ExpiryDate
        {
            get { return CurrentData.ExpiryDate; }
        }

        public IFund Fund
        {
            get { return CurrentData.Fund; }
        }

        OrderStateMachine.State IOrderComputedData.OrderState
        {
            get { return CurrentData.OrderState; }
        }

        /// <summary>Business status derived from the state and the pending trigger (New, PartiallyFilled, CancelPending, ...).</summary>
        public OrderStatus OrderStatus
        {
            get { return CurrentData.OrderStatus; }
        }

        /// <summary>Quantity minus the executed quantity.</summary>
        public double RemainingQuantity
        {
            get { return CurrentData.RemainingQuantity; }
        }
    }
}