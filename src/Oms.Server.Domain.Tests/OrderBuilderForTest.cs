using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.Domain.Tests
{
    public class OrderBuilderForTest : OrderBuilder
    {
       
        private Queue<ITrade> trades = new Queue<ITrade>();
        private OrderStateMachine.State? _orderStateOverride;
        private DateTime _currentDateTime = DateTime.Now;



        public OrderBuilderForTest WithTrade(Trade trade)
        {
            trades.Enqueue(trade);
            return this;
        }

        public OrderBuilderForTest WithOrderState(OrderStateMachine.State orderState)
        {
            _orderStateOverride = orderState;
            return this;
        }

        private DateTime GetNextDateTime()
        {
            return _currentDateTime = _currentDateTime.AddMinutes(1);
        }

        private TriggerContext CreateTriggerContext()
        {
            return new TriggerContext(new User(), GetNextDateTime());
        }

        private void AddDataEvent(Order order)
        {
            order.EventLogs.Add(new OrderParameterEventLog<IOrderEditableData>(CreateTriggerContext(), OrderStateMachine.Trigger.Create, TriggerStatus.Done, _orderStateOverride ?? OrderStateMachine.State.Undefined, order, _editableData));
        }

        private void AddDealEvents(Order order)
        {
            foreach (var trade in trades)
            {
                order.EventLogs.Add(new OrderParameterEventLog<IOrderDealingData>(CreateTriggerContext(), OrderStateMachine.Trigger.AddTrade, TriggerStatus.Done, _orderStateOverride ?? OrderStateMachine.State.Undefined, order, new OrderDealingEventParameter(trade)));
            }
        }

        public override Order Build()
        {
            var order = new Order();
            AddDataEvent(order);
            AddDealEvents(order);
            return order;
        }
    }
}
