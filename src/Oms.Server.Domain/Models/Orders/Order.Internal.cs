using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Instruments;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Models.Orders
{
    public partial class Order
    {
        private OrderData _currentDataDoNotUseDirectly;
        private OrderStateMachine _stateMachineDoNotUserDirectly;
        private const double Epsilon = 0.000001;

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
            if (orderData == null)
            {
                _stateMachineDoNotUserDirectly = null;
                //TODO compute all the data with eventlogs
                _currentDataDoNotUseDirectly = new OrderData();
            }
            else
            {
                _currentDataDoNotUseDirectly = orderData;
                StateMachine.Initialize(CurrentData.OrderState);
            }
        }

        private OrderStateMachine StateMachine
        {
            get
            {
                if (_stateMachineDoNotUserDirectly == null)
                {
                    _stateMachineDoNotUserDirectly = new OrderStateMachine(true);
                    _stateMachineDoNotUserDirectly.Initialize(CurrentData.OrderState);
                }
                return _stateMachineDoNotUserDirectly;
            }
        }

        private double ComputeRemainingQuantity()
        {
            return EventLogs.OfType<DataEventLog<OrderStateMachine.Trigger, OrderDealingData>>()
                .Select(e => e.Data.Trade)
                .Where(t => t.TradeStatus != TradeStatus.Cancelled)
                .Sum(t => t.Side == Side ? t.ExecutionQuantity : -t.ExecutionQuantity);
        }

        private OrderStatus ComputeOrderStatus(OrderData orderData)
        {
            switch (orderData.OrderState)
            {
                case OrderStateMachine.State.Terminated:
                    return IsFilled(orderData) ? OrderStatus.CancelledFilled : OrderStatus.Cancelled;
                case OrderStateMachine.State.Booking:
                case OrderStateMachine.State.Validating:
                    if (orderData.RemainingQuantity < Epsilon)
                        return OrderStatus.OverFilled;
                    if(IsFilled(orderData))
                        return OrderStatus.Filled;
                    goto case OrderStateMachine.State.Dealing;
                case OrderStateMachine.State.Dealing:
                case OrderStateMachine.State.Working:
                    if(orderData.RemainingQuantity >= Epsilon)
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

        private bool IsFilled(OrderData orderData)
        {
            return Math.Abs(workingData.Quantity - workingData.RemainingQuantity) < Epsilon;
        }

        private class OrderData : IOrderTransientData, IOrderRoutingData, ICloneable
        {
            public OrderData()
            {
            }

            public OrderData SetTransientData(IOrderTransientData transientData)
            {
                if (transientData != null)
                {
                    Quantity = transientData.Quantity;
                    Price = transientData.Price;
                    Side = transientData.Side;
                    Instrument = transientData.Instrument;
                    OrderType = transientData.OrderType;
                    OrderValidity = transientData.OrderValidity;
                    ExpiryDate = transientData.ExpiryDate;
                    Fund = transientData.Fund;
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

            public OrderData Clone()
            {
                var result = new OrderData().SetRoutingData(this).SetTransientData(this);
                result.OrderState = this.OrderState;
                result.PendingTrigger = this.PendingTrigger;
                result.RemainingQuantity = this.RemainingQuantity;
                result.OrderStatus = this.OrderStatus;
                return result;
            }

            object ICloneable.Clone()
            {
                return Clone();
            }

            public double Quantity { get; set; }
            public double Price { get; set; }
            public Side Side { get; set; }
            public Instrument Instrument { get; set; }
            public OrderType OrderType { get; set; }
            public OrderValidityType OrderValidity { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public OrderStateMachine.State OrderState { get; set; }
            public string StateMachineErrorMessage { get; set; }
            public Fund Fund { get; set; }
            public OrderStateMachine.Trigger? PendingTrigger { get; set; }
            public double RemainingQuantity { get; set; }
            public OrderStatus OrderStatus { get; set; }
        }
    }
}
