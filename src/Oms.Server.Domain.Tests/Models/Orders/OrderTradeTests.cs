using System.Linq;
using NUnit.Framework;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Workflow;
using State = Oms.Server.Domain.Workflow.OrderStateMachine.State;

namespace Oms.Server.Domain.Tests.Models.Orders
{
    [TestFixture]
    public class OrderTradeTests
    {
        private static Trade ExecutedTrade(double quantity)
        {
            var trade = new Trade();
            trade.Create(new TriggerContext(), new TradeEditableEventParameter { ExecutionQuantity = quantity });
            return trade;
        }

        private static Order CreateOrder(State state, double quantity, params Trade[] trades)
        {
            var builder = new OrderBuilderForTest().WithOrderState(state);
            foreach (var trade in trades)
                builder.WithTrade(trade);
            return builder
                .WithCoreData(new User(), new User(), new OrderBasket(), "ref")
                .WithFund(new Fund())
                .WithSecurity(new Security())
                .WithPrices(OrderType.Limit, quantity, 10, null)
                .WithInitialReferentialData("instCode", SecurityCodeType.Isin, "fundCode", FundCodeType.Other, "folioCode")
                .Build();
        }

        private static GenericResult AddTrade(Order order, double quantity)
        {
            // The trade passed in only carries the data of the execution; the order creates its own.
            return order.AddTrade(new TriggerContext(), new Trade { ExecutionQuantity = quantity });
        }

        [TestCase(State.Working)]
        [TestCase(State.Dealing)]
        public void Should_move_to_or_stay_in_dealing_while_the_order_is_not_fully_executed(State from)
        {
            var order = CreateOrder(from, 100);

            var result = AddTrade(order, 40);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(State.Dealing));
            Assert.That(order.RemainingQuantity, Is.EqualTo(60));
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.PartiallyFilled));
        }

        [TestCase(State.Working, 100)]
        [TestCase(State.Dealing, 100)]
        public void Should_move_to_validating_when_the_order_is_fully_executed(State from, double quantity)
        {
            var order = CreateOrder(from, 100);

            var result = AddTrade(order, quantity);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(State.Validating));
            Assert.That(order.RemainingQuantity, Is.EqualTo(0));
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Filled));
        }

        [Test]
        public void Should_validate_the_order_once_the_last_trade_completes_it()
        {
            var order = CreateOrder(State.Working, 100);

            AddTrade(order, 40);
            Assert.That(order.OrderState, Is.EqualTo(State.Dealing));
            AddTrade(order, 60);

            Assert.That(order.OrderState, Is.EqualTo(State.Validating));
            Assert.That(order.RemainingQuantity, Is.EqualTo(0));
        }

        [Test]
        public void Should_move_to_validating_and_report_an_over_fill()
        {
            var order = CreateOrder(State.Working, 100);

            AddTrade(order, 130);

            Assert.That(order.OrderState, Is.EqualTo(State.Validating));
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.OverFilled));
        }

        [Test]
        public void Should_log_the_trade_owned_by_the_order()
        {
            var order = CreateOrder(State.Working, 100);
            var data = new Trade { ExecutionQuantity = 40 };

            order.AddTrade(new TriggerContext(), data);

            var logged = ((OrderParameterEventLog<IOrderDealingData>)order.EventLogs.Last()).Parameters.Trade;
            Assert.That(order.EventLogs.Last().Trigger, Is.EqualTo(OrderStateMachine.Trigger.AddTrade));
            Assert.That(logged, Is.Not.SameAs(data));
            Assert.That(logged.Order, Is.SameAs(order));
            Assert.That(logged.TradeState, Is.EqualTo(TradeState.New));
            Assert.That(logged.ExecutionQuantity, Is.EqualTo(40));
        }

        [Test]
        public void Should_refuse_a_trade_without_quantity()
        {
            var order = CreateOrder(State.Working, 100);
            var eventCount = order.EventLogs.Count;

            var result = AddTrade(order, 0);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(order.OrderState, Is.EqualTo(State.Working));
            Assert.That(order.EventLogs.Count, Is.EqualTo(eventCount));
        }

        [TestCase(State.Draft)]
        [TestCase(State.Accepting)]
        public void Should_refuse_a_trade_before_the_order_is_working(State from)
        {
            var order = CreateOrder(from, 100);

            var result = AddTrade(order, 40);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(order.OrderState, Is.EqualTo(from));
        }

        [TestCase(State.Validating)]
        [TestCase(State.Booking)]
        public void Should_go_back_to_dealing_when_cancelling_a_trade_leaves_the_order_unexecuted(State from)
        {
            var cancelled = ExecutedTrade(40);
            var order = CreateOrder(from, 100, ExecutedTrade(60), cancelled);

            var result = order.CancelTrade(new TriggerContext(), cancelled);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(State.Dealing));
            Assert.That(order.RemainingQuantity, Is.EqualTo(40));
        }

        [TestCase(State.Validating)]
        [TestCase(State.Booking)]
        public void Should_stay_put_when_the_order_is_still_fully_executed_after_cancelling_a_trade(State from)
        {
            var cancelled = ExecutedTrade(30);
            var order = CreateOrder(from, 100, ExecutedTrade(100), cancelled);

            var result = order.CancelTrade(new TriggerContext(), cancelled);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(from));
            Assert.That(order.RemainingQuantity, Is.EqualTo(0));
        }

        private static Trade SentTrade(double quantity)
        {
            var trade = ExecutedTrade(quantity);
            trade.BookingSend(new TriggerContext());
            return trade;
        }

        [Test]
        public void Should_stay_in_booking_until_the_last_trade_is_booked()
        {
            var first = SentTrade(60);
            var second = SentTrade(40);
            var order = CreateOrder(State.Booking, 100, first, second);

            Assert.That(order.TradeBooked(new TriggerContext(), first).IsSuccess(), Is.True);
            Assert.That(first.TradeState, Is.EqualTo(TradeState.Booked));
            Assert.That(order.OrderState, Is.EqualTo(State.Booking));

            var result = order.TradeBooked(new TriggerContext(), second);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(State.Terminated));
            Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Filled));
            Assert.That(order.EventLogs.Last().Trigger, Is.EqualTo(OrderStateMachine.Trigger.TradeBooked));
        }

        [Test]
        public void Should_accept_a_trade_that_was_forced_to_booked()
        {
            var trade = ExecutedTrade(100);
            var order = CreateOrder(State.Booking, 100, trade);
            trade.ForceBooked(new TriggerContext());

            var result = order.TradeBooked(new TriggerContext(), trade);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
            Assert.That(order.OrderState, Is.EqualTo(State.Terminated));
        }

        [Test]
        public void Should_ignore_cancelled_trades_when_deciding_that_everything_is_booked()
        {
            var booked = SentTrade(60);
            var cancelled = ExecutedTrade(40);
            var order = CreateOrder(State.Booking, 100, booked, cancelled);
            cancelled.Cancel(new TriggerContext());

            order.TradeBooked(new TriggerContext(), booked);

            Assert.That(order.OrderState, Is.EqualTo(State.Terminated));
        }

        [Test]
        public void Should_refuse_a_trade_that_was_not_sent_to_booking()
        {
            var trade = ExecutedTrade(100);
            var order = CreateOrder(State.Booking, 100, trade);
            var eventCount = order.EventLogs.Count;

            var result = order.TradeBooked(new TriggerContext(), trade);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(trade.TradeState, Is.EqualTo(TradeState.New));
            Assert.That(order.OrderState, Is.EqualTo(State.Booking));
            Assert.That(order.EventLogs.Count, Is.EqualTo(eventCount));
        }

        [Test]
        public void Should_refuse_a_booked_trade_before_the_order_is_booking()
        {
            var trade = SentTrade(100);
            var order = CreateOrder(State.Validating, 100, trade);

            var result = order.TradeBooked(new TriggerContext(), trade);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(order.OrderState, Is.EqualTo(State.Validating));
        }

        [Test]
        public void Should_refuse_a_booking_or_a_cancellation_without_a_trade()
        {
            var order = CreateOrder(State.Booking, 100, SentTrade(100));

            Assert.That(order.TradeBooked(new TriggerContext(), null).IsFailure(), Is.True);
            Assert.That(order.CancelTrade(new TriggerContext(), null).IsFailure(), Is.True);
        }
    }
}
