using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Instruments;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.Domain.Models.Orders
{
    public partial class Order
    {
        private OrderData _currentDataDoNotUseDirectly;
        private OrderStateMachine _stateMachineDoNotUserDirectly;

        private OrderData CurrentData
        {
            get
            {
                if (_currentDataDoNotUseDirectly == null)
                    _currentDataDoNotUseDirectly = ComputeCurrentData();
                return _currentDataDoNotUseDirectly;
            }
        }
        private OrderData ComputeCurrentData(OrderData orderData = null)
        {
            _stateMachineDoNotUserDirectly = null;
            return orderData ?? new OrderData();
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

        private class OrderData : IOrderTransientData, IOrderDealingData, ICloneable
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
                    Way = transientData.Way;
                    Instrument = transientData.Instrument;
                    OrderType = transientData.OrderType;
                    OrderValidity = transientData.OrderValidity;
                    ExpiryDate = transientData.ExpiryDate;
                    Fund = transientData.Fund;
                }
                return this;
            }

            public OrderData SetDealingData(IOrderDealingData dealingData)
            {
                if (dealingData != null)
                {
                }
                return this;
            }

            public OrderData Clone()
            {
                var result = new OrderData().SetDealingData(this).SetTransientData(this);
                result.OrderState = this.OrderState;
                return result;
            }

            object ICloneable.Clone() { return Clone(); }

            public double Quantity { get; set; }
            public double Price { get; set; }
            public OrderWay Way { get; set; }
            public Instrument Instrument { get; set; }
            public OrderType OrderType { get; set; }
            public OrderValidityType OrderValidity { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public OrderStateMachine.State OrderState { get; set; }
            public string StateMachineErrorMessage { get; set; }
            public Fund Fund { get; set; }
        }
    }
}
