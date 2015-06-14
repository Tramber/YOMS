using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Instruments;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.Orders
{
    public partial class Order : IOrderTransientData, IOrderDealingData, IOrderComputedData
    {

        public OrderStateMachine.State OrderState
        {
            get { return StateMachine.GetState; }
        }

        public OrderStateMachine.Trigger? PendingTrigger
        {
            get { return CurrentData.PendingTrigger; }
        }

        public double Quantity
        {
            get { return CurrentData.Quantity; }
        }

        public double Price
        {
            get { return CurrentData.Price; }
        }

        public OrderWay Way
        {
            get { return CurrentData.Way; }
        }

        public Instrument Instrument
        {
            get { return CurrentData.Instrument; }
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

        public Fund Fund
        {
            get { return CurrentData.Fund; }
        }

        OrderStateMachine.Trigger IOrderComputedData.OrderState
        {
            get { throw new NotImplementedException(); }
        }

        public OrderStatus OrderStatus
        {
            get { throw new NotImplementedException(); }
        }

        public double RemainingQuantity
        {
            get { throw new NotImplementedException(); }
        }
    }
}