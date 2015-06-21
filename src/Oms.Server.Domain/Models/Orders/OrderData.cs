using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.Orders
{
    internal class OrderData : IOrderEditableData, IOrderRoutingData, ICloneable
    {
        public double Quantity { get; private set; }
        public double? PriceLimit { get; private set; }
        public Side Side { get; private set; }
        public ISecurity Security { get; private set; }
        public OrderType OrderType { get; private set; }
        public OrderValidityType OrderValidity { get; private set; }
        public DateTime? ExpiryDate { get; private set; }
        public OrderStateMachine.State OrderState { get; private set; }
        public string StateMachineErrorMessage { get; private set; }
        public IFund Fund { get; private set; }
        public double? PriceStop { get; set; }
        public DateTime? SettlementDate { get; set; }
        public OrderStateMachine.Trigger? PendingTrigger { get; private set; }
        public double RemainingQuantity { get; private set; }
        public OrderStatus OrderStatus { get; private set; }
        public OrderStateMachine.Trigger Trigger { get; private set; }

        public OrderData SetEditableData(IOrderEditableData editableData)
        {
            if (editableData != null)
            {
                Quantity = editableData.Quantity;
                PriceLimit = editableData.PriceLimit;
                Side = editableData.Side;
                Security = editableData.Security;
                OrderType = editableData.OrderType;
                OrderValidity = editableData.OrderValidity;
                ExpiryDate = editableData.ExpiryDate;
                Fund = editableData.Fund;
                PriceStop = editableData.PriceStop;
                SettlementDate = editableData.SettlementDate;
            }
            return this;
        }

        public OrderData SetRoutingData(IOrderRoutingData routingData)
        {
            if (routingData != null)
            {
            }
            return this;
        }

        public OrderData SetOrderState(OrderStateMachine.State orderState)
        {
            this.OrderState = orderState;
            return this;
        }

        public void SetExecutionQuantity(double executionQuantity)
        {
            this.RemainingQuantity = Quantity - executionQuantity;
        }

        public OrderData SetPendingTrigger(OrderStateMachine.Trigger? pendingTrigger)
        {
            this.PendingTrigger = pendingTrigger;
            return this;
        }

        public OrderData SetOrderStatus(OrderStatus orderStatus)
        {
            this.OrderStatus = orderStatus;
            return this;
        }

        public OrderData SetTrigger(OrderStateMachine.Trigger trigger)
        {
            this.Trigger = trigger;
            return this;
        }

        public OrderData Clone()
        {
            var result = new OrderData().SetRoutingData(this).SetEditableData(this);
            result.OrderState = this.OrderState;
            result.PendingTrigger = this.PendingTrigger;
            result.RemainingQuantity = this.RemainingQuantity;
            result.OrderStatus = this.OrderStatus;
            result.Trigger = this.Trigger; 
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}