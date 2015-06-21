using System;
using System.Collections.Generic;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.Orders
{
    public partial class Order : IOrderEditableData, IOrderComputedData, IOrderRoutingData
    {
        public int Id { get; set; }
        public IList<OrderEventLog> EventLogs { get; set; }
        public IUser Owner { get; internal set; }
        public IUser Creator { get; internal set; }
        public DateTime CreationDate { get; internal set; }
        public IOrderBasket OrderBasket { get; internal set; }
        public IOrderInitialReferentialData InitialReferentialData { get; internal set; }
        public string ClientOrderRef { get; internal set; }
        public bool IsAutoAccepted { get; internal set; }

        public OrderStateMachine.State OrderState
        {
            get { return _stateMachine.GetState; }
        }

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

        public OrderStatus OrderStatus
        {
            get { return CurrentData.OrderStatus; }
        }

        public double RemainingQuantity
        {
            get { return CurrentData.RemainingQuantity; }
        }
    }
}